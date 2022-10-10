using System.Collections;
using UnityEngine;

public class MusicController : Singleton<MusicController> {

    private enum Track
    {
        Station, Ambient, Battle
    }

    // TWo sources are needed for crossfading
    public AudioSource source1, source2, effects;
    public AudioClip ambient, battle, station;
    [Tooltip("How often the Music Controller will check the distance between the player ship and" +
        "the closest enemy ship to change music")]
    public float checkInterval = 2f;
    [Tooltip("Distance below which the Battle music replaces Ambient music")]
    public int battleDistanceThreshold = 750;
    [Tooltip("Set this to play battle music regardless of distance")]
    public bool forceBattle;
    public float crossfadeDuration = 3f;

    private float _checkTimer;
    private Track _currentTrack = Track.Ambient;
    private bool _isSwitching;
    public float globalMusicVolume = 1f;
    public float globalSoundVolume = 1f;
    
    private void Start()
    {
        _checkTimer = checkInterval;
        // All clear, chill out mode
        StartCoroutine(SwitchTrack(ambient, Track.Ambient));
    }

    private void Update()
    {
        _checkTimer -= Time.deltaTime;
        if(_checkTimer < 0)
        {
            _checkTimer = checkInterval;          
            if (Ship.PlayerShip == null)
                return;

            if (Ship.PlayerShip.stationDocked != "none" && _currentTrack != Track.Station)
            {
                StartCoroutine(SwitchTrack(station, Track.Station));
            }
            if (Ship.PlayerShip.stationDocked == "none")
            {
                // Check distance to closest enemy ship
                var transform1 = Ship.PlayerShip.transform;
                var playerPosition = transform1.position;
                var enemies = SectorNavigation.GetClosestNPCShip(transform1, 5000);
                if (enemies.Count == 0 && _currentTrack != Track.Ambient)
                {
                    // No enemy in range.
                    StartCoroutine(SwitchTrack(ambient, Track.Ambient));
                }
                if (enemies.Count > 0){
                    var closestNPCDist = Vector3.Distance(playerPosition, enemies[0].transform.position);

                    if (_currentTrack != Track.Ambient && closestNPCDist > battleDistanceThreshold)
                        // All clear, chill out mode
                        StartCoroutine(SwitchTrack(ambient, Track.Ambient));
                    if (_currentTrack != Track.Battle && closestNPCDist < battleDistanceThreshold)
                        // Danger close, start war drums
                        StartCoroutine(SwitchTrack(battle, Track.Battle));
                    if (_currentTrack != Track.Battle && forceBattle)
                        // Danger close, start war drums
                        StartCoroutine(SwitchTrack(battle, Track.Battle));
                }
            }
            source1.volume = globalMusicVolume;
            source2.volume = globalMusicVolume;
            effects.volume = globalSoundVolume;
        }
    }

    private IEnumerator SwitchTrack(AudioClip clipToPlay, Track track)
    {
        if (_isSwitching)
            yield return null;

        _isSwitching = true;
        AudioSource newSource, oldSource;
        _currentTrack = track;

        if((int)source1.volume == 1)
        {
            newSource = source2;
            oldSource = source1;
        }
        else
        {
            newSource = source1;
            oldSource = source2;
        }

        newSource.clip = clipToPlay;
        newSource.Play();
        newSource.volume = 0;

        while(newSource.volume < 1.0)
        {
            newSource.volume += Time.deltaTime / crossfadeDuration;
            if (oldSource.isPlaying && oldSource.volume >= 0)
                oldSource.volume -= Time.deltaTime / crossfadeDuration;

            yield return null;
        }

        oldSource.Stop();
        newSource.volume = 1.0f;
        _isSwitching = false;

        // Crossfade
        yield return null;
    }

    public void PlaySound(AudioClip clip)
    {
        effects.volume = globalSoundVolume;
        effects.PlayOneShot(clip);
    }

}
