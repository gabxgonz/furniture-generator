using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class StartScreen : MonoBehaviour {
  public string initialScene;
  private Dictionary<string, AudioSource> soundClipByName;
  // private AudioSource [] soundClips;

  public void Start () {
    InitSound ();
  }

  public void StartButton () {
    StartGame();
  }

  public void StartGame () {
    if (initialScene.Length == 0) {
      Debug.LogWarning("Add initial scene to StartScreen script.");
    } else {
      SceneManager.LoadScene(initialScene);
    }
  }

  private void InitSound () {
    // soundClips = gameObject.GetComponents<AudioSource>();
    // soundClipByName = new Dictionary<string, AudioSource>();
    // soundClipByName.Add ("button", soundClips[0]);
  }
}
