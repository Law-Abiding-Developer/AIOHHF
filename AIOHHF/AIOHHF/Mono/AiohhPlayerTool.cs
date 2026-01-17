using System;
using AIOHHF.Items.Equipment;
using UnityEngine;


namespace AIOHHF.Mono;

public class AiohhPlayerTool : PlayerTool
{
    public AioHandHeldFabricator fab;
    public PowerRelay relay;
    public HandHeldBatterySource battery;
    public StorageContainer storageContainer;
    private double _counter;
    private GameObject _hands;
    public Transform leftHand;
    public Transform rightHand;
    public override void Awake()
    {
        //socket = Socket.Camera;
        fab = gameObject.GetComponent<AioHandHeldFabricator>();
        relay = gameObject.GetComponent<HandHeldRelay>();
        fab.powerRelay = relay;
        battery = gameObject.GetComponent<HandHeldBatterySource>();
        storageContainer = gameObject.GetComponent<StorageContainer>();
        pickupable = gameObject.GetComponent<Pickupable>();
        battery.connectedRelay = relay;
        relay.AddInboundPower(battery);
        _hands = Plugin.Aiohhf.Bundle.LoadAsset<GameObject>("Hands");
        Instantiate(_hands, Player.main.gameObject.transform);
        leftHand = _hands.FindChild("left_hand").transform;
        rightHand = _hands.FindChild("right_hand").transform;
        base.Awake();
        ikAimRightArm = true;
    }

    public override bool OnRightHandDown()
    {
        fab.opened = true;
        fab.animator.SetBool(AnimatorHashID.open_fabricator, true);
        _counter = 0f;
        uGUI.main.craftingMenu.Open(Plugin.Aiohhf.TreeType, fab);
        return true;
    }

    public override bool OnAltDown()
    {
        if (!storageContainer.open && storageContainer != null && storageContainer.container != null)
        {
            storageContainer.container._label =  "ALL IN ONE HAND HELD FABRICATOR";
            storageContainer.Open();
        }

        return true;
    }

    public void Update()
    {
            gameObject.transform.localScale = Plugin.Aiohhf.PostScaleValue;
            /*_hands.transform.localEulerAngles = new Vector3(0,180,0);
            _hands.transform.localPosition = new Vector3(0, -0.1f, 0.25f);*/
            _counter += Time.deltaTime;
            if (_counter >= 7f)
            {
                fab.animator.SetBool(AnimatorHashID.open_fabricator, false);
                _counter = 0;
            }
            else if (uGUI.main.craftingMenu.isActiveAndEnabled)
            {
                fab.animator.SetBool(AnimatorHashID.open_fabricator, true);
                _counter = 0;
            }

            if (fab.crafterLogic.inProgress && !fab.animator.GetBool(AnimatorHashID.open_fabricator))
            {
                fab.animator.SetBool(AnimatorHashID.open_fabricator, true);
            }
    }

    public override void OnDraw(Player p)
    {
        base.OnDraw(p);
        gameObject.transform.CopyTransformPosition(_hands.transform);
        if (fab.animator == null) return;
        fab.animator.SetBool(AnimatorHashID.open_fabricator, true);
        gameObject.FindChild("collision").SetActive(false);
        if (leftHand != null && rightHand != null)
        {
            //leftHandIKTarget = leftHand;
            //rightHandIKTarget = rightHand;
        }
        _counter = 0;
    }

    public override void OnHolster()
    {
        base.OnHolster();
        if (fab.animator == null) return;
        fab.animator.SetBool(AnimatorHashID.open_fabricator, false);
        _counter = 0;
    }
}