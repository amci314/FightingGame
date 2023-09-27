using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class adsScript : MonoBehaviour
{
    [SerializeField] private bool testMode = true;

    private string gameId = "5337950";

    private void Start()
    {
        Advertisement.Initialize(gameId, testMode);
    }

    public static void ShowAdsVideo()
    {
        Advertisement.Show("Interstitial_Android");
    }
}
