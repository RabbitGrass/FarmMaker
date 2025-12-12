using System.Collections;
using UnityEngine;

public class BenchInteractSimple : MonoBehaviour
{
    private bool playerInRange = false;
    private PlayerController player;
    public bool isSitting = false;

    [Header("Bench A Settings")]
    [SerializeField] private Vector3 playerWorldPosA = new Vector3(33.1227f, 8.85251f, 89.28792f);
    [SerializeField] private Vector3 playerWorldRotA = new Vector3(0f, 21.802f, 0f);
    [SerializeField] private Vector3 mainCharLocalPosA = new Vector3(1.9f, -0.48f, 0.23f);
    [SerializeField] private Vector3 mainCharLocalRotA = new Vector3(0f, -91.579f, 0f);

    [Header("Bench B Settings")]
    [SerializeField] private Vector3 playerWorldPosB = new Vector3(-94.99f, -0.16f, 25.2652f);
    [SerializeField] private Vector3 playerWorldRotB = new Vector3(0f, 259.91f, 0f);
    [SerializeField] private Vector3 mainCharLocalPosB = new Vector3(0f, -1.268f, 0f);
    [SerializeField] private Vector3 mainCharLocalRotB = new Vector3(0f, 15.004f, 0f);

    [Header("Bench C Settings")]
    [SerializeField] private Vector3 playerWorldPosC = new Vector3(100f, 41.74f, -21.94f);
    [SerializeField] private Vector3 playerWorldRotC = new Vector3(0f, 301.339f, 0f);
    [SerializeField] private Vector3 mainCharLocalPosC = new Vector3(0f, -1.268f, 0f);
    [SerializeField] private Vector3 mainCharLocalRotC = new Vector3(0f, 0f, 0f);

    //public GameObject interactUI;

    private Vector3 savedPlayerPos;
    private Quaternion savedPlayerRot;
    private Vector3 savedMainCharLocalPos;
    private Quaternion savedMainCharLocalRot;

    private string currentBenchTag; // 현재 앉은 벤치 태그

    private void Start()
    {
        //if (interactUI != null)
            //interactUI.SetActive(false);
        Debug.Log("BenchInteractSimple.cs");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("휴식취하기 온트리거엔터");
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        player = other.GetComponent<PlayerController>();
        //if (interactUI != null) interactUI.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isSitting) return;
        playerInRange = false;
        player = null;
        //if (interactUI != null) interactUI.SetActive(false);
    }

    //private void Update()
    //{
    //    if (!playerInRange || player == null) return;

    //    if (Input.GetKeyDown(KeyCode.F))
    //    {
    //        if (!isSitting)
    //            SitPlayer();
    //        else
    //            StandPlayer();
    //    }
    //    //Debug.Log($"Rot={player.Rot}");
    //}

    public void SitPlayer()
    {
        currentBenchTag = gameObject.tag;

        // 현재 벤치 참조 전달
        InGameBGMSoundManager.Instance.benchInteract = this;

        if (player == null) return;
        isSitting = true;

        savedPlayerPos = player.transform.position;
        savedPlayerRot = player.transform.rotation;

        Transform mainChar = player.transform.Find("MainChar");
        if (mainChar != null)
        {
            savedMainCharLocalPos = mainChar.localPosition;
            savedMainCharLocalRot = mainChar.localRotation;
        }

        player.anim.SetBool("Sit", true);
        player.state = PlayerController.State.idle;
        player.moveSpeed = 0;

        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Vector3 targetPlayerPos, targetPlayerRot;
        Vector3 targetMainPos, targetMainRot;

        // 벤치별 위치 지정 후 BGM 변경
        AudioClip targetBGM = null;

        if (CompareTag("BenchA"))
        {
            targetPlayerPos = playerWorldPosA;
            targetPlayerRot = playerWorldRotA;
            targetMainPos = mainCharLocalPosA;
            targetMainRot = mainCharLocalRotA;
        }
        else if (CompareTag("BenchB"))
        {
            targetPlayerPos = playerWorldPosB;
            targetPlayerRot = playerWorldRotB;
            targetMainPos = mainCharLocalPosB;
            targetMainRot = mainCharLocalRotB;
        }
        else
        {
            targetPlayerPos = playerWorldPosC;
            targetPlayerRot = playerWorldRotC;
            targetMainPos = mainCharLocalPosC;
            targetMainRot = mainCharLocalRotC;
        }

        if (InGameBGMSoundManager.Instance != null)
        {
            InGameBGMSoundManager.Instance?.SetVolumeForBench(true);  // 앉을 때
            InGameBGMSoundManager.Instance.PlayBenchBGM(currentBenchTag);
        }

        // 위치 & 회전 동기화 (Transform + Rot)
        player.transform.SetPositionAndRotation(targetPlayerPos, Quaternion.Euler(targetPlayerRot));
        player.Rot = targetPlayerRot.y;
        player.ForceApplyRotation(); // 즉시 적용 (Update() 덮어쓰기 방지)

        if (mainChar != null)
        {
            mainChar.localPosition = targetMainPos;
            mainChar.localRotation = Quaternion.Euler(targetMainRot);
        }

        //if (interactUI != null)
            //interactUI.SetActive(false);

        //Debug.Log($"[Bench] Sit -> Rot={player.Rot}");
    }

    public void StandPlayer()
    {
        currentBenchTag = null;
        InGameBGMSoundManager.Instance.benchInteract = null;

        if (player == null) return;

        isSitting = false;
        player.anim.SetBool("Sit", false);

        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 위치 복원
        player.transform.SetPositionAndRotation(savedPlayerPos, savedPlayerRot);

        //  핵심: Rot을 Transform 회전값으로 덮어씌우고, 즉시 적용
        player.Rot = player.transform.eulerAngles.y;
        player.ForceApplyRotation();

        Transform mainChar = player.transform.Find("MainChar");
        if (mainChar != null)
        {
            mainChar.localPosition = savedMainCharLocalPos;
            mainChar.localRotation = savedMainCharLocalRot;
        }

        // 바닥 보정
        if (Physics.Raycast(player.transform.position + Vector3.up, Vector3.down, out RaycastHit hit, 5f, LayerMask.GetMask("Ground")))
        {
            player.transform.position = new Vector3(player.transform.position.x, hit.point.y, player.transform.position.z);
        }

        if (cc != null) cc.enabled = true;
        player.moveSpeed = player.basicMoveSpeed;

        //if (interactUI != null)
            //interactUI.SetActive(true);

        //  회전 복원 중에는 PlayerController.Update() 회전 갱신 막기
        player.isRestoringRotation = true;
        player.Rot = player.transform.eulerAngles.y;
        player.ForceApplyRotation();
        player.SnapRotation();
        player.StartCoroutine(DelayReleaseRotation());

        if (InGameBGMSoundManager.Instance != null)
        {
            InGameBGMSoundManager.Instance?.SetVolumeForBench(false); // 일어날 때
            InGameBGMSoundManager.Instance.RestoreOriginalBGM();
        }

        //Debug.Log($"[Bench] Stand -> Rot={player.Rot}");
    }

    private IEnumerator DelayReleaseRotation()
    {
        yield return new WaitForSeconds(0.05f); // 1~2프레임 대기
        if (player != null)
        {
            player.ForceSyncRotation(); // 최종 동기화
            player.SnapRotation();
            player.isRestoringRotation = false;
        }
    }
}