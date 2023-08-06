using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace CraftableMobNS
{
    public class Craftable : Blueprint
    {
      public override void BlueprintComplete(GameCard rootCard, List<GameCard> involvedCards, Subprint print)
      {
        foreach (GameCard card in involvedCards)
       {
           if (card.CardData.Id == Cards.stew)
           {
               Food stew = (Food)card.CardData;
               stew.FoodValue -= 2; // or change this to 1
               if (stew.FoodValue <= 0)
                   stew.MyGameCard.DestroyCard();
               else
                   stew.MyGameCard.SendIt();
           }
           else if (card.CardData.Id == Cards.rat)
           {
               card.DestroyCard();
           }
       }
       var spawnedCard = WorldManager.instance.CreateCard(rootCard.transform.position, print.ResultCard, true, false);
       // uncomment the following line if you want the spawned card to also bounce like the stew
       spawnedCard.MyGameCard.SendIt();
      }
    }

    public class CraftableMob : Mod
    {
        public override void Ready()
        {
            Logger.Log("Ready!");
            Harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Mob),  "CanHaveCard")]
    class Patch
    {
      static void Postfix(CardData otherCard, ref Mob __instance, ref bool __result)
      {
        if (otherCard.Id == "stew" && __instance.Id == "rat")
        {
          __result = true;
        }
      }
    }

    [HarmonyPatch(typeof(Resource),  "CanHaveCard")]
    class Patch2
    {
      static void Postfix(CardData otherCard, ref Resource __instance, ref bool __result)
      {
        if ((otherCard.Id == "coin_chest" || otherCard.Id == "shell_chest") && __instance.Id == "magic_dust")
        {
          __result = true;
        }
      }
    }

    [HarmonyPatch(typeof(Chest),  "CanHaveCard")]
    class Patch5
    {
      static void Postfix(CardData otherCard, ref Chest __instance, ref bool __result)
      {
        if ((__instance.Id == "coin_chest" || __instance.Id == "shell_chest") && otherCard.Id == "magic_dust")
        {
          __result = true;
        }
      }
    }

    [HarmonyPatch(typeof(TreasureChest),  "CanHaveCard")]
    class Patch52
    {
      static void Postfix(CardData otherCard, ref TreasureChest __instance, ref bool __result)
      {
        if (__instance.Id == "treasure_chest" && otherCard.Id == "magic_dust")
        {
          __result = true;
        }
      }
    }

    /*[HarmonyPatch(typeof(Draggable), "SendIt")]
    class PatchPoulet2
    {
      static void Postfix(ref Draggable __instance)
      {
        Debug.Log("NONNONONOO");

        //__instance.RotWobble(5f);

        Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * 20f * 1.5f;
        __instance.Velocity = new Vector3(vector.x, 20f, vector.y);
      }*/
}
