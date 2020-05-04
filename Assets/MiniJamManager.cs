using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MiniJamManager : MonoBehaviour
{
    public static MiniJamManager instance = null;

    public GameObject Player {  get { return m_player; } }

    [SerializeField]
    private Song m_mainSong = null;

    [SerializeField]
    private GameObject m_player = null;

    [SerializeField]
    private GameObject m_castle = null;

    [SerializeField]
    private Shop m_shop = null;

    [SerializeField]
    private TextMeshProUGUI m_statusText = null;

    [SerializeField]
    private float m_statusStayTimeSec = 1.0f;

    [SerializeField]
    private int m_waveTimeSecStart = 60;

    [SerializeField]
    private float m_waveTimeMult = 1.5f;

    [SerializeField]
    private TextMeshProUGUI m_waveText = null;

    [SerializeField]
    private TextMeshProUGUI m_minionPriceText = null;

    [SerializeField]
    private TextMeshProUGUI m_castlePriceText = null;

    [SerializeField]
    private TextMeshProUGUI m_healPriceText = null;

    [SerializeField]
    float m_cameraGrowPerMinion = 0.2f;

    [SerializeField]
    private List<Spawner> m_spawnerList = new List<Spawner>();

    private int m_healPrice = 1;
    private int m_waveTimeSec = 60;

    public int Wave {
        get { return m_wave; }
        set {
            m_wave = value;
            m_waveText.text = "" + m_wave;

            foreach ( var spawner in m_spawnerList ) spawner.enabled = false;

            m_waveTimeSec = m_waveTimeSecStart;
            for ( int i = 0; i < Mathf.Min( m_wave, m_spawnerList.Count ); ++i ) {
                if( i > 0 ) m_waveTimeSec = Mathf.FloorToInt( m_waveTimeSec * m_waveTimeMult );
                m_spawnerList[i].enabled = true;
            }

            m_timer.SetStart( 0, 0, m_waveTimeSec );
            m_timer.ResetTime();

            AddOrRemoveStem();
        }
    }

    private float m_playerHealthEnteringCastle = 0.0f;
    private float m_initialHealth = 10.0f;
    private int m_wave = 1;
    private float m_timeSinceLastStatusChange = 0.0f;
    private ElapsedTimer m_timer = null;

    private bool m_isInCastle = false;
    private float m_cameraStartSize = 1.0f;

    private string StatusText {
        set {
            m_statusText.text = value;
            m_timeSinceLastStatusChange = 0.0f;
        }
    }

    public void BuyMinion() {
        if ( m_isInCastle ) {
            StatusText = "You can't make purchases in the castle.";
            return;
        }

        var spent = m_shop.Purchase( "Minion" );
        if ( spent >= 0 ) {
            Camera.main.orthographicSize += m_cameraGrowPerMinion;
            StatusText = "You buy a minion.";
        } else StatusText = "You can't afford a minion.";

        UpdatePriceDisplay();
    }

    public void BuyCastle() {
        if ( m_isInCastle ) {
            LeaveCastle();
            StatusText = "You leave the castle.";
            return;
        }

        var spent = m_shop.Purchase( "Castle" );
        if ( spent >= 0 ) {
            m_playerHealthEnteringCastle = m_player.GetComponent<Health>().Count;
            m_castle.transform.position = m_player.transform.position;

            var castleHealth = m_castle.GetComponent<Health>();
            castleHealth.SetRange( 0, m_player.GetComponent<Health>().Maximum );
            castleHealth.ResetToMaximum();

            m_player.SetActive( false );
            m_castle.SetActive( true );

            m_isInCastle = true;
            StatusText = "A castle is constructed, and you hole up in safety... for now.";
            GetComponent<MoveController>().TargetMover = m_castle.GetComponent<Mover>();
            UpdatePriceDisplay();
        } else StatusText = "You can't afford a castle.";
    }

    public void HandleDeath() {
        var minionList = FindObjectsOfType<Minion>();
        var coins = Counter.FindByName( "Coins" ).Count;
        var multBonus = ( m_wave  - 1 ) * 0.2f + minionList.Length * 0.1f + coins * 0.01f;
        Debug.LogFormat( "Mult bonus: {0}", multBonus );
        Counter.FindByName( "Deaths" ).Add( multBonus );
        NewRound();
    }

    public void HealPlayer() {
        if ( m_isInCastle ) {
            StatusText = "You can't heal in the castle.";
            return;
        }

        var minionList = FindObjectsOfType<Minion>();
        if ( minionList.Length < m_healPrice ) {
            StatusText = "Not enough minions; need " + m_healPrice + " to heal";
            return;
        }

        var player = GameObject.FindGameObjectWithTag( "Player" );
        var health = player.GetComponent<Health>();
        health.SetRange( 0, health.Maximum + m_healPrice );
        health.ResetToMaximum();
        for( int i = 0; i < m_healPrice; ++i )
            Destroy( minionList[i].gameObject );

        Camera.main.orthographicSize -= m_cameraGrowPerMinion * m_healPrice;
        ++m_healPrice;

        UpdatePriceDisplay();
    }

    public void LeaveCastle() {
        StatusText = "The castle was destroyed!";
        m_castle.SetActive( false );
        m_player.transform.position = m_castle.transform.position;
        m_player.SetActive( true );
        m_isInCastle = false;
        m_player.GetComponent<Health>().Count = m_playerHealthEnteringCastle;

        GetComponent<MoveController>().TargetMover = m_player.GetComponent<Mover>();
    }

    public void ResetGame() {
        Counter.FindByName( "Coins" ).Count = 0.0f;
        Counter.FindByName( "Deaths" ).Count = 1.0f;
        Player.GetComponent<WeaponWielder>().Weapon.GetComponent<Damager>().Damage = 1;
        m_player.GetComponent<Health>().SetRange( 0.0f, 10.0f );
        m_healPrice = 1;

        SaveGame();
        NewRound();
    }

    private void Awake() {
        if( instance != null ) {
            Destroy( this );
            return;
        }
        instance = this;

        m_cameraStartSize = Camera.main.orthographicSize;
        m_timer = GetComponent<ElapsedTimer>();
    }

    private void Start() {
        m_mainSong.Play();
        NewRound();
        UpdatePriceDisplay();

        LoadGame();
        m_initialHealth = m_player.GetComponent<Health>().Maximum;
    }

    private float m_timePerSave = 1.0f;
    private float m_timeSinceLastSave = 0.0f;

    private void Update() {
        m_timeSinceLastSave += Time.deltaTime;
        if( m_timeSinceLastSave >= m_timePerSave ) {
            SaveGame();
            m_timeSinceLastSave = 0.0f;
        }

        m_timeSinceLastStatusChange += Time.deltaTime;

        if ( m_timeSinceLastStatusChange < m_statusStayTimeSec ) return;
        StatusText = "";

        if( m_timer.HasEnded ) ++Wave;
    }

    public void NextWave() { ++Wave; }

    private void LoadGame() {
        if ( !PlayerPrefs.HasKey( "Coins" ) || !PlayerPrefs.HasKey( "Deaths" ) || !PlayerPrefs.HasKey( "Health" ) ) {
            ResetGame();
            return;
        }

        Counter.FindByName( "Coins" ).Count = PlayerPrefs.GetFloat( "Coins" );
        Counter.FindByName( "Deaths" ).Count = PlayerPrefs.GetFloat( "Deaths" );

        var damager = Player.GetComponent<WeaponWielder>().Weapon.GetComponent<Damager>();
        damager.Damage = PlayerPrefs.GetFloat( "Damage" );

        var health = m_player.GetComponent<Health>();
        health.SetRange( 0.0f, PlayerPrefs.GetFloat( "Health" ) );
        health.ResetToMaximum();
    }

    private void SaveGame() {
        PlayerPrefs.SetFloat( "Coins", Counter.FindByName( "Coins" ).Count );
        PlayerPrefs.SetFloat( "Deaths", Counter.FindByName( "Deaths" ).Count );

        var health = m_player.GetComponent<Health>();
        PlayerPrefs.SetFloat( "Health", health.Maximum );

        var damager = Player.GetComponent<WeaponWielder>().Weapon.GetComponent<Damager>();
        PlayerPrefs.SetFloat( "Damage", damager.Damage );
    }

    private bool m_addingMusic = true;

    private void AddOrRemoveStem() {
        if ( m_mainSong.FullyActivated ) m_addingMusic = false;

        Debug.Log( "Adding music? " + m_addingMusic );

        if( m_addingMusic) m_mainSong.ActivateRandomInactiveStem();
        else m_mainSong.DeactivateRandomStem();

        if ( m_mainSong.NoneActivated ) {
            m_addingMusic = true;
            m_mainSong.ActivateRandomInactiveStem();
        }
    }

    private void NewRound() {
        Camera.main.orthographicSize = m_cameraStartSize;
        m_mainSong.DeactivateAllStems();

        m_shop.ResetPrices();
        UpdatePriceDisplay();
        m_timer.ResetTime();
        Wave = 1;

        Counter.FindByName( "Coins" ).Count = 0;

        var player = GameObject.FindGameObjectWithTag( "Player" );

        var health = player.GetComponent<Health>();
        //health.SetRange( 0, m_initialHealth );
        health.ResetToMaximum();

        player.transform.position = Vector3.zero;
        player.GetComponent<Knockback>().CancelKnockback();

        foreach( var collectible in FindObjectsOfType<Collectible>() )
            Destroy( collectible.gameObject );

        foreach( var ai in FindObjectsOfType<AiMover3d>() )
            Destroy( ai.gameObject );
    }

    private void UpdatePriceDisplay() {
        m_minionPriceText.text = "" + m_shop.GetPrice( "Minion" );
        m_castlePriceText.text = "" + m_shop.GetPrice( "Castle" );
        m_healPriceText.text = "" + m_healPrice;
    }
}
