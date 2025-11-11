using AIOHHF.Items.Equipment;
using UnityEngine;


namespace AIOHHF.Mono;

public class AiohhPlayerTool : PlayerTool
{
    public AioHandHeldFabricator fab;
    public PowerRelay relay;
    public HandHeldBatterySource battery;
    public StorageContainer storageContainer;
    private double _counter = 0;
    public override void Awake()
    {
        fab = gameObject.GetComponent<AioHandHeldFabricator>();
        relay = gameObject.GetComponent<PowerRelay>();
        fab.powerRelay = relay;
        battery = gameObject.GetComponent<HandHeldBatterySource>();
        storageContainer = gameObject.GetComponent<StorageContainer>();
        pickupable = gameObject.GetComponent<Pickupable>();
        battery.connectedRelay = relay;
        relay.AddInboundPower(battery);
        ikAimLeftArm = true;
        ikAimRightArm = true;
        base.Awake();
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
        _counter += Time.deltaTime;
        if (_counter >= 7f
            && !uGUI.main.craftingMenu.isActiveAndEnabled)
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

        if (isDrawn)
        {
            var x = MainCamera.camera.transform.position.x;
            var y = MainCamera.camera.transform.position.y;
            var z = MainCamera.camera.transform.position.z;
            gameObject.transform.position = new Vector3(x, y, z);
            gameObject.transform.localPosition = new Vector3(0.5f,0,0.5f);
            gameObject.transform.eulerAngles = -MainCamera.camera.transform.eulerAngles;
        }
    }

    public override void OnDraw(Player p)
    {
        base.OnDraw(p);
        if (fab.animator == null) return;
        fab.animator.SetBool(AnimatorHashID.open_fabricator, true);
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