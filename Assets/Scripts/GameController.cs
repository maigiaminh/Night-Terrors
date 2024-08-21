using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class GameController : MonoBehaviour
{
  public PlayerController player;
  public DoorManager door;
  public ClosetManager closet;
  public EnemyController enemyController;
  public GameObject jumpscare;
  public GameObject windowLight;
  public GameObject enemy;
  public GameObject animationHolder, canvas, skyLight;
  DialogueController dialogueController;
  readonly Dialogues dialogues = new();
  FirstCutscene firstCutscene;
  SecondCutscene secondCutscene;
  GameObject pickedUpObject;
  public GameObject target;

  ItemDictionary itemDictionary;
  public int stage = 1;
  bool playStage = true;
  public bool isLoadScene = false;

  // JumpscareScene jumpscareScene;
  // Start is called before the first frame update

  void Start()
  {
    itemDictionary = new ItemDictionary();
    firstCutscene = gameObject.AddComponent<FirstCutscene>();
    secondCutscene = gameObject.AddComponent<SecondCutscene>();
    // jumpscareScene = gameObject.AddComponent<JumpscareScene>();

    door = GameObject.FindGameObjectWithTag("Door").GetComponent<DoorManager>();
    closet = GameObject.FindGameObjectWithTag("Closet").transform.parent.Find("door_L").GetComponent<ClosetManager>();
    enemyController = enemy.GetComponent<EnemyController>();


    dialogueController = gameObject.AddComponent<DialogueController>();
    dialogueController.canvas = canvas;
    dialogueController.player = player.GetComponent<PlayerController>();
    skyLight.SetActive(false);
    jumpscare.SetActive(false);

    if(isLoadScene){
      Debug.Log("Load scene from Game Controller");
      LoadScene();
    }
  }

  // Update is called once per frame
  void Update()
  {
    Debug.Log("Stage:" + stage);
    // if (Input.GetKeyDown(KeyCode.T))
    // {
    //   StartCoroutine(jumpscareScene.Play(player.gameObject, enemy, animationHolder));
    // }
    if(!isLoadScene){
      if (stage == 1 && playStage)
      {
        StartCoroutine(firstCutscene.Play(dialogueController, player.gameObject));

        stage = 2;
        playStage = false;
      }
      if (stage == 2 && playStage)
      {
        HidePrompt();
        StartCoroutine(dialogueController.ShowDialogue(dialogues.dialogues[1]));

        GameObject.Find("Pill Bottle Pickup").gameObject.layer = LayerMask.NameToLayer("Interactive Objects");
        playStage = false;
        stage = 3;
      }
      if (stage == 3 && playStage)
      {
        HidePrompt();
        StartCoroutine(dialogueController.ShowDialogue(dialogues.dialogues[2]));
        GameObject.Find("Bed").layer = LayerMask.NameToLayer("Interactive Objects");

        playStage = false;
        stage = 4;
      }
      if (stage == 4 && playStage)
      {
        HidePrompt();
        StartCoroutine(secondCutscene.Play(dialogueController, player.gameObject));
        door.switchState(door.doorHalfCloseState);
        stage = 5;
        skyLight.SetActive(true);

        playStage = false;
      }

      if (secondCutscene.lightExploded)
      {
        secondCutscene.lightExploded = false;
        
        enemyController.StartEnemyBehaviour();
        isLoadScene = true;
      }
    }
    
  }

  private void LoadScene(){
      GameObject light = GameObject.Find("Room Light");
      GameObject foodTray = GameObject.Find("Food Tray");

      foodTray.layer = LayerMask.NameToLayer("Default");
      foodTray.transform.localPosition = new Vector3(-16.84f, 1.81f, -9.47855f);
      foodTray.transform.GetChild(0).GameObject().SetActive(false);      

      light.GetComponent<LightBehavior>().DisableLight();
      enemyController.StartEnemyBehaviour();

  }

  public void ShowPrompt(GameObject hitObject)
  {
    if ((hitObject.CompareTag("Player") && player.GetIsHolding()) ||
        (hitObject.CompareTag("Target") && !player.GetIsHolding()) || hitObject.CompareTag("Window") && !player.GetIsHolding())
    {
      canvas.transform.Find("Prompt").GetComponent<TMP_Text>().text = "";
      return;
    }
    canvas.transform.Find("Prompt").GetComponent<TMP_Text>().text = "E to interact";
  }

  public void HidePrompt()
  {
    canvas.transform.Find("Prompt").GetComponent<TMP_Text>().text = "";
  }

  public void InteractObject(GameObject hitObject)
  {
    //Change this condition to debug
    if (stage < 5)
    {
      if (hitObject.CompareTag("Door"))
      {
        if (door.currentState.GetType().Equals(typeof(DoorCloseState)))
        {
          door.switchState(door.doorOpenState);
        }
        else
        {
          door.switchState(door.doorCloseState);
        }
      }
      if (hitObject.CompareTag("Item") && !player.GetIsHolding())
      {
        player.SetIsHolding(true);
        Instantiate(target, itemDictionary.itemDictionary[hitObject.name], Quaternion.identity);
        pickedUpObject = hitObject;
        pickedUpObject.AddComponent<PickupBehavior>();
        pickedUpObject.GetComponent<PickupBehavior>().parent = player.gameObject;
      }
      if (hitObject.CompareTag("Target") && player.GetIsHolding())
      {
        player.SetIsHolding(false);
        Destroy(pickedUpObject.GetComponent<PickupBehavior>());
        pickedUpObject.transform.parent = GameObject.Find("Room").transform;
        pickedUpObject.transform.SetPositionAndRotation(hitObject.transform.position, hitObject.transform.rotation);
        pickedUpObject.layer = LayerMask.NameToLayer("Default");
        Destroy(hitObject.transform.parent.gameObject);
        playStage = true;
      }
      if (hitObject.CompareTag("Bed"))
      {
        playStage = true;
        hitObject.layer = LayerMask.NameToLayer("Default");
      }
    }
    else
    {
      if (hitObject.CompareTag("Door"))
      {
        if (door.currentState.GetType().Equals(typeof(DoorHalfOpenState)))
        {
          door.switchState(door.doorHalfCloseState);
        }
        else
        {
          door.switchState(door.doorHalfOpenState);
        }
      }

      if (hitObject.CompareTag("Closet"))
      {
        // Debug.Log(hitObject);
        if (closet.currentState.GetType().Equals(typeof(ClosetCloseState)) || closet.currentState.GetType().Equals(typeof(ClosetHalfCloseState)))
        {
          closet.switchState(closet.closetOpenState);
        }
        else if (closet.currentState.GetType().Equals(typeof(ClosetOpenState)))
        {
          closet.switchState(closet.closetCloseState);
        }
        else if (closet.currentState.GetType().Equals(typeof(ClosetHalfOpenState)) && enemyController.IsEnemyInCloset())
        {
          closet.switchState(closet.closetHalfCloseState);
        }
        else
        {
          closet.switchState(closet.closetOpenState);
        }

      }
    }
  }

  public void holdingDoor(GameObject hitObject)
  {
    if (hitObject.CompareTag("Door"))
    {
      door.switchState(door.doorHalfCloseState);
    }

    if (hitObject.CompareTag("Closet"))
    {
      if (closet.currentState.GetType().Equals(typeof(ClosetHalfOpenState)))
      {
        // Debug.Log(isHeld + "false and door close");
        closet.switchState(closet.closetHalfCloseState);
      }
      else if (closet.currentState.GetType().Equals(typeof(ClosetCloseState)) || closet.currentState.GetType().Equals(typeof(ClosetHalfCloseState)))
      {
        // Debug.Log(isHeld + "false and door open");
        closet.switchState(closet.closetHalfOpenState);
      }
    }
  }
}
