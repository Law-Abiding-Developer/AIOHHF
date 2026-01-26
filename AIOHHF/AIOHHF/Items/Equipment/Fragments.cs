using System;
using System.Collections;
using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UWE;
using Object = UnityEngine.Object;

namespace AIOHHF.Items;

public static class Fragments
{
    private static PrefabInfo[] _fragmentPIs = new PrefabInfo[3];
    private static CustomPrefab[] _fragmentCPs = new CustomPrefab[3];
    public static TechType FragmentsTechType;
    public static IEnumerator Initialize(WaitScreenHandler.WaitScreenTask task)
    {
        var fragments = FragmentsTechType = EnumHandler.AddEntry<TechType>("AIOHHFFragment").Value;
        var biomesToSpawnIn = new List<LootDistributionData.BiomeData>()
        {
            new LootDistributionData.BiomeData()
            {
                biome = BiomeType.Kelp_Sand,
                probability = 0.05f,
                count = 1
            },
            new LootDistributionData.BiomeData()
            {
                biome = BiomeType.Kelp_GrassSparse,
                probability = 0.05f,
                count = 1
            }
        };
        foreach (BiomeType item in Enum.GetValues(typeof(BiomeType)))
        {
            if (!(item.ToString().Contains("Obsolete") || item.ToString().Contains("Unused"))
                && (item.ToString().Contains("Tech")
                    || item.ToString().Contains("EscapePod")
                    || item.ToString().Contains("Ship")
                    || item.ToString().Contains("Aurora")))
            {
                biomesToSpawnIn.Add(new LootDistributionData.BiomeData()
                {
                    biome = item,
                    probability = 0.05f,
                    count = 1
                });
            }
        }
        
        for (var i = 0; i < 3; i++)
        {
            _fragmentPIs[i] = new PrefabInfo("AIOHHFF" + i, "aiohhffragprefab" + i, fragments);
            var WEI = new WorldEntityInfo()
                                            {
                                                techType = fragments,
                                                classId = _fragmentPIs[i].ClassID,
                                                localScale = Vector3.one,
                                                slotType = EntitySlot.Type.Small,
                                                cellLevel = LargeWorldEntity.CellLevel.Global,
                                                prefabZUp = false
                                            };
            _fragmentCPs[i] = new CustomPrefab(_fragmentPIs[i]);
            _fragmentCPs[i].SetSpawns(WEI, biomesToSpawnIn.ToArray());
            var i1 = i;
            _fragmentCPs[i].SetGameObject(() =>
            {
                GameObject fragment =
                    Object.Instantiate(Plugin.Aiohhf.Bundle.LoadAsset<GameObject>("aiohhffragprefab1"));
                fragment.SetActive(false);
                PrefabUtils.AddBasicComponents(fragment, _fragmentPIs[i1].ClassID, _fragmentPIs[i1].TechType,
                    LargeWorldEntity.CellLevel.Global);
                MaterialUtils.ApplySNShaders(fragment);
                var rb = fragment.AddComponent<Rigidbody>();
                rb.mass = 5f;
                rb.useGravity = false;
                rb.isKinematic = true;
                var wf =  fragment.GetComponent<WorldForces>();
                wf.useRigidbody = rb;
                return fragment;
            });
            _fragmentCPs[i].CreateFragment(Plugin.Aiohhf.PrefabInfo.TechType, 3f);
            _fragmentCPs[i].Register();
        }
        yield return null;
    }
}
    