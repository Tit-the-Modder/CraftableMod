using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;
using System.Reflection;

namespace CraftableMobNS
{
    public class CraftableMob : Mod
    {
      public void Awake()
      {
        //Logger.Log("Awake!");
        TrapUtil.initDict();
        Harmony.PatchAll(typeof(CraftableMob));
      }
      public override void Ready()
      {
        WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.Island_AdvancedIdea, "craftable_mob_rumor_trap", 1);
      }

      [HarmonyPatch(typeof(ForestCombatManager),  "PrepareWave")]
      [HarmonyPostfix]
      public static void DropWitchIdea()
      {
        var wickedwitchidea = "craftable_mob_blueprint_" + Cards.wicked_witch;
        Debug.Log(WorldManager.instance.CurrentRunVariables.FinishedWickedWitch);
        if (!(WorldManager.instance.HasFoundCard(wickedwitchidea)) && (WorldManager.instance.CurrentRunVariables.FinishedWickedWitch ||  WorldManager.instance.CurrentRunVariables.ForestWave > ForestCombatManager.instance.WickedWitchWave))
        {
          var createcard = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), wickedwitchidea, true, false);
          createcard.MyGameCard.SendIt();
        }
      }
      [HarmonyPatch(typeof(StrangePortal),  "SpawnCreature")]
      [HarmonyPostfix]
      public static void TryDropIdeaPortals(CardData __instance)
      {
        var Idea = new CardData();
        WorldManager.instance.GameDataLoader.idToCard.TryGetValue("craftable_mob_blueprint_" + __instance.Id, out Idea);
        if (!(Idea == null || WorldManager.instance.HasFoundCard(Idea.Id)))
        {
          var createcard = WorldManager.instance.CreateCard(__instance.transform.position, Idea.Id, true, false);
          createcard.MyGameCard.SendIt();
        }
      }
      [HarmonyPatch(typeof(Mob),  "TryDropItems")]
      [HarmonyPostfix]
      public static void TryDropIdeaRumor(CardData __instance)
      {
        // Debug.Log(WorldManager.instance.GameDataLoader.idToCard.Join());
        var Idea = new CardData();
        if (WorldManager.instance.GameDataLoader.idToCard.ContainsKey("craftable_mob_blueprint_" + __instance.Id))
          Idea = WorldManager.instance.GameDataLoader.idToCard["craftable_mob_blueprint_" + __instance.Id];
        else if (WorldManager.instance.GameDataLoader.idToCard.ContainsKey("craftable_mob_rumor_trap_" + __instance.Id))
          Idea = WorldManager.instance.GameDataLoader.idToCard["craftable_mob_rumor_trap_" + __instance.Id];

        //Debug.Log(Idea);
        if (!(Idea == null || WorldManager.instance.HasFoundCard(Idea.Id)))
        {
          var createcard = WorldManager.instance.CreateCard(__instance.transform.position, Idea.Id, true, false);
          createcard.MyGameCard.SendIt();
        }
      }


      [HarmonyPatch(typeof(Equipable), "TryEquipOnCard")]
      [HarmonyPrefix]
      static bool notequipelves(GameCard card, CardData __instance)
      {
        if (card != null)
        {
          if(TrapUtil.MobEquipChange.ContainsKey(card.CardData.Id))
          {
            if (__instance is Equipable equip && TrapUtil.MobEquipChange[card.CardData.Id].Contains(equip.VillagerTypeOverride))
            {
              return false;
            }
          }
        }
        return true;
      }

      [HarmonyPatch(typeof(CardData), "GetDelegateForActionId")]
      [HarmonyPrefix]
      public static bool trapId(string id, CardData __instance, ref TimerAction __result)
      {
        if (id == "complete_trap")
        {
        MethodInfo method = typeof(HarvestableExtensions).GetMethod(nameof(HarvestableExtensions.CompleteTrap));
        __result = (TimerAction)method.CreateDelegate(typeof(TimerAction), __instance);
        }
        else if (id == "complete_bait")
        {
          MethodInfo method = typeof(HarvestableExtensions).GetMethod(nameof(HarvestableExtensions.CompleteBait));
          __result = (TimerAction)method.CreateDelegate(typeof(TimerAction), __instance);
        }
        else
        {
          return true;
        }
        return false;
      }

      [HarmonyPatch(typeof(CardData),  "CanHaveCardsWhileHasStatus")]
      [HarmonyPostfix]
      static void trapstatus(ref CardData __instance, ref bool __result)
      {
        if (__instance is Harvestable)
        {
          List<GameCard> IsRope = __instance.MyGameCard.GetChildCards();
          foreach (GameCard child in IsRope)
          {
            if (child.CardData.Id == Cards.rope)
            {
              __result = true;
            }
          }
        }
      }
      [HarmonyPatch(typeof(Harvestable),  "CanHaveCard")]
      [HarmonyPostfix]
      static void trap(CardData otherCard, ref bool __result, ref CardData __instance)
      {
        if (otherCard.Id == Cards.rope || otherCard is Food || TrapUtil.NotFoodBait.Contains(otherCard.Id) || (otherCard.Id == Cards.kid && __instance.Id == Cards.forest) || otherCard is Equipable equip && equip.AttackType == AttackType.Magic)
        {
          __result = true;
        }
      }

      [HarmonyPatch(typeof(Mob),  "CanHaveCard")]
      [HarmonyPostfix]
      public static void rat(CardData otherCard, ref Mob __instance, ref bool __result)
      {
        if (otherCard.Id == Cards.stew && __instance.Id == Cards.rat)
        {
          __result = true;
        }
        if (otherCard.Id == Cards.goop && __instance.Id == Cards.small_slime && otherCard.MyGameCard.GetRootCard().GetChildCount() + 1 == 3)
        {
          __result = true;
        }
        if(TrapUtil.MobEquipChange.ContainsKey(__instance.Id))
        {
          if(WorldManager.instance.GameDataLoader.GetCardFromId(otherCard.Id) is Equipable equip && TrapUtil.MobEquipChange[__instance.Id].Contains(equip.VillagerTypeOverride))
          {
            __result = true;
          }
        }
      }

      [HarmonyPatch(typeof(Resource),  "CanHaveCard")]
      [HarmonyPostfix]
      public static void mimic2(CardData otherCard, ref Resource __instance, ref bool __result)
      {
        if ((otherCard.Id == Cards.coin_chest || otherCard.Id == Cards.shell_chest) && __instance.Id == Cards.magic_dust)
        {
          __result = true;
        }
      }

      [HarmonyPatch(typeof(Chest),  "CanHaveCard")]
      [HarmonyPostfix]
      public static void mimic5(CardData otherCard, ref Chest __instance, ref bool __result)
      {
        if ((__instance.Id == Cards.coin_chest || __instance.Id == Cards.shell_chest) && otherCard.Id == Cards.magic_dust)
        {
          __result = true;
        }
      }
      [HarmonyPatch(typeof(TreasureChest),  "CanHaveCard")]
      [HarmonyPostfix]
      public static void mimic52(CardData otherCard, ref bool __result)
      {
        if (otherCard.Id == Cards.magic_dust)
        {
          __result = true;
        }
      }

      [HarmonyPatch(typeof(Harvestable),  "UpdateCard")]
      [HarmonyPrefix]
      static void Prefix(Harvestable __instance)
      {
        if (__instance.MyGameCard.StackUpdate)
        {
          bool allgood = false;
          bool baitgood = false;
          bool first = true;
          CardData bait = new CardData();
          allgood = false;
          baitgood = false;
          TrapUtil.baitlist = __instance.MyGameCard.GetChildCards();
          TrapUtil.baitchance = __instance.MyCardBag.Chances;

          foreach (GameCard child in TrapUtil.baitlist)
          {
            CardData fchild = child.CardData;
            if (fchild is BaseVillager/* && (__instance.Id == Cards.spring || __instance.Id == Cards.well)*/)
            {
              fchild = WorldManager.instance.GameDataLoader.GetCardFromId(Cards.villager);
            }
            if (fchild is Equipable equip && equip.AttackType == AttackType.Magic)
            {
              fchild = WorldManager.instance.GameDataLoader.GetCardFromId(Cards.magic_wand);
            }
            if (fchild.Id == Cards.rope)
            {
              foreach(CardChance c in TrapUtil.baitchance)
              {
                if(c.IsEnemy || WorldManager.instance.GameDataLoader.GetCardFromId(c.Id) is Mob )
                {
                  allgood = true;
                  break;
                }
              }
              if (TrapUtil.BaitToMob.ContainsKey(fchild.Id))
              {
                if (TrapUtil.BaitException.ContainsKey(__instance.Id))
                {
                  allgood = true;
                }
                baitgood = true;
                if(first)
                {
                  bait = fchild;
                  first = false;
                }
              }
            }
            if (fchild is Food || TrapUtil.NotFoodBait.Contains(fchild.Id))
            {
              baitgood = true;
              if(first)
              {
                bait = fchild;
                first = false;
              }
            }
          }
          if (allgood)
          {
            ICardId cardId = new CardId(TrapUtil.DefaultMob);
            if (TrapUtil.BaitToMob.ContainsKey(bait.Id))
            {
              cardId = TrapUtil.BaitToMob[bait.Id];
            }
            if (baitgood && TrapUtil.CheckHarvestable(cardId, __instance))
            {
              __instance.MyGameCard.StartTimer(TrapUtil.baittime,
            () => {
                HarvestableExtensions.CompleteBait(__instance);
                }, SokLoc.Translate("craftable_mod_idea_trap_status"), "complete_bait");
            }
            else
            {
              float TrueTrapTime;
              float TotalChance = 0;
              float EnemyChance = 0;
              foreach (CardChance chance in TrapUtil.baitchance)
              {
                TotalChance += chance.Chance;
                if (WorldManager.instance.GameDataLoader.GetCardFromId(chance.Id) is Mob || chance.IsEnemy)
                  EnemyChance += chance.Chance;
              }
              var x = EnemyChance/TotalChance;
              TrueTrapTime = (float)Math.Round(Mathf.Lerp(TrapUtil.traptime,120,(float)Math.Pow(1-x, TrapUtil.traptimepow))); // see wich combinaition is the greatest
              __instance.MyGameCard.StartTimer(TrueTrapTime,
              () => {
                HarvestableExtensions.CompleteTrap(__instance);
              }, SokLoc.Translate("craftable_mod_idea_trap_status"), "complete_trap");
            }
          }
          else
          {
            __instance.MyGameCard.CancelTimer("complete_bait");
          __instance.MyGameCard.CancelTimer("complete_trap");
          }
        }
      }
    }
    //For the rat to only eat a portion of the soup
    public class Bigrat : Blueprint
    {
      public override void BlueprintComplete(GameCard rootCard, List<GameCard> involvedCards, Subprint print)
      {
        bool first = true;
        foreach (GameCard card in involvedCards)
        {
           if (card.CardData.Id == Cards.stew && first)
           {
               first = false;
               Food stew = (Food)card.CardData;
               stew.FoodValue -= 3; // to see wich value is good
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
       spawnedCard.MyGameCard.SendIt();
      }
    }

    public class LotofSubprints : Blueprint
    {
      public override void Init(GameDataLoader loader)
      {
        var name = this.NameTerm;
        var Attneed = new List<string>();
        if (name[0] == new string("m")[0])
        {
          Attneed.Add("wizard");
          Attneed.Add("mage");
        }
        else if (name[0] == new string("r")[0])
        {
          Attneed.Add("archer");
        }
        var mobneed = name.Substring(2, name.IndexOf("_", 2) - 2);
        name = name.Substring(name.IndexOf("_", 2) + 1);
        this.NameTerm = new string("card_" + name + "_name");
        //Debug.Log(mobneed + " name: " + name);
        var i = 0;
        foreach (string card in Cards.all)
        {
          if (loader.GetCardFromId(card) is Equipable equip && Attneed.Contains(equip.VillagerTypeOverride))
          {
            Equipable cardequip = loader.GetCardFromId(card) as Equipable;
            this.Subprints.Add(new Subprint{
				    RequiredCards = new string[2] {mobneed, card},
				    ResultCard = name,
				    Time = 0f,
				    StatusTerm = "haha"
            });
            //Debug.Log(cardequip.VillagerTypeOverride);
            //Debug.Log(this.Subprints[i].RequiredCards.Join() + "  " + this.Subprints[i].ResultCard);
            Subprint subprint = this.Subprints[i];
            subprint.ParentBlueprint = this;
            subprint.SubprintIndex = i;
            i+=1;
          }
        }
      }
    }

    public class WickedWithEquipment : Blueprint
    {
      public override void BlueprintComplete(GameCard rootCard, List<GameCard> involvedCards, Subprint print)
      {
        foreach (GameCard card in involvedCards)
        {
          card.DestroyCard();
        }
        var spawnedCard = WorldManager.instance.CreateCard(rootCard.transform.position, print.ResultCard, true, false);
        var equip = WorldManager.instance.CreateCard(rootCard.transform.position, Cards.magic_broom, false ,false ,false);
        spawnedCard.MyGameCard.EquipmentChildren.Add(equip.MyGameCard);
        equip.MyGameCard.EquipmentHolder = spawnedCard.MyGameCard;
        spawnedCard.MyGameCard.SendIt();
      }
    }
    public static class HarvestableExtensions
    {
      [TimedAction("complete_trap")]
      public static void CompleteTrap(this Harvestable card)
      {
        var loader = WorldManager.instance.GameDataLoader;
        List<CardId> moblist = new List<CardId>();
        foreach (CardChance chance in card.MyCardBag.Chances)
        {
          if (WorldManager.instance.GameDataLoader.GetCardFromId(chance.Id) is Mob)
          {
            moblist.Add(new CardId(chance.Id));
          }
          else if (chance.IsEnemy)
          {
            SetCardBagType setCardBagForEnemyCardBag = loader.GetSetCardBagForEnemyCardBag(chance.EnemyBag);
            List<CardChance> chancesForSetCardBag = CardBag.GetChancesForSetCardBag(loader, setCardBagForEnemyCardBag);
            chancesForSetCardBag.RemoveAll((CardChance x) => ((loader.GetCardFromId(x.Id) as Combatable).ProcessedCombatStats.CombatLevel > chance.Strength) ? true : false);
            foreach(CardChance c in chancesForSetCardBag)
            {
              moblist.Add(new CardId(c.Id));
            }
          }
        }
        var spawnedCard = WorldManager.instance.CreateCard(card.transform.position, moblist[UnityEngine.Random.Range(0, moblist.Count)], true, false);
        spawnedCard.MyGameCard.SendIt();
        card.MyGameCard.StackUpdate = true;
      }

      [TimedAction("complete_bait")]
      public static void CompleteBait(this Harvestable card)
      {
        CardData bait = null;
        ICardId cardId = new CardId(TrapUtil.DefaultMob);
        foreach (GameCard child in card.MyGameCard.GetChildCards())
        {
          if (child.CardData is Food || TrapUtil.NotFoodBait.Contains(child.CardData.Id))
            bait = child.CardData;
        }
        if (TrapUtil.BaitToMob.ContainsKey(bait.Id))
        {
          cardId = TrapUtil.BaitToMob[bait.Id];
        }
        CardData spawned = WorldManager.instance.CreateCard(card.transform.position, cardId, true, false);
        spawned.MyGameCard.SendIt();
        if (bait is BaseVillager)
        {
          bait.MyGameCard.Combatable.Damage(3);
          bait.MyGameCard.SendIt();
        }
        else if (bait.Id == Cards.bottle_of_water)
        {
          var newcard = WorldManager.instance.ChangeToCard(bait.MyGameCard, Cards.empty_bottle);
          newcard.MyGameCard.RemoveFromParent();
          newcard.MyGameCard.SendIt();
        }
        else
          bait.MyGameCard.DestroyCard();
      }
    }

    public class TrapUtil : Harvestable
    {
      public static List<GameCard> baitlist = new List<GameCard>();
      public static List<CardChance> baitchance = new List<CardChance>();
      public static string DefaultMob = Cards.chicken;
      public static float baittime = 30f; // experiment to see wich is the best
      public static float traptime = 60f; // same
      public static float traptimepow = 2.5f; // same
      public static Dictionary<string, List<string>> BaitException = new Dictionary<string, List<string>>() {{Cards.forest, new List<string>() {Cards.bear, Cards.giant_snail, Cards.wolf, Cards.ogre, Cards.feral_cat}}};
      public static Dictionary<string, ICardId> BaitToMob = new Dictionary<string, ICardId>(){{Cards.raw_meat,new CardId(Cards.bear)}};
      public static Dictionary<string, List<string>> MobEquipChange = new Dictionary<string, List<string>>() {{Cards.elf, new List<string>() {"archer", "wizard", "mage"}}};
      public static List<string> NotFoodBait = new List<string>() {Cards.goop, Cards.rabbit, Cards.bone, Cards.gold_bar, Cards.magic_wand, Cards.villager, Cards.chicken, Cards.cow, Cards.sheep, Cards.bottle_of_water};

      public static void initDict()
      {
        TrapUtil.BaitToMob.Add(Cards.seaweed, new CardId(Cards.giant_snail));
        TrapUtil.BaitToMob.Add(Cards.villager, new CardId(Cards.mosquito));
        TrapUtil.BaitToMob.Add(Cards.goop, new CardId(Cards.rat));
        TrapUtil.BaitToMob.Add(Cards.raw_fish, new CardId(Cards.seagull));
        TrapUtil.BaitToMob.Add(Cards.rabbit, new CardId(Cards.snake));
        TrapUtil.BaitToMob.Add(Cards.bone, new CardId(Cards.wolf));
        TrapUtil.BaitToMob.Add(Cards.carrot, new CardId(Cards.rabbit));
        TrapUtil.BaitToMob.Add(Cards.gold_bar, new CardId(Cards.goblin));
        TrapUtil.BaitToMob.Add(Cards.cow, new CardId(Cards.ogre));
        TrapUtil.BaitToMob.Add(Cards.sheep, new CardId(Cards.ogre));
        TrapUtil.BaitToMob.Add(Cards.magic_wand, new CardId(Cards.orc_wizard));
        TrapUtil.BaitToMob.Add(Cards.banana, new CardId(Cards.monkey));
        TrapUtil.BaitToMob.Add(Cards.milk, new CardId(Cards.feral_cat));
        TrapUtil.BaitToMob.Add(Cards.chicken, new CardId(Cards.tiger));
        TrapUtil.BaitToMob.Add(Cards.milkshake, new CardId(Cards.merman));
        TrapUtil.BaitToMob.Add(Cards.bottle_of_water, new CardId(Cards.frog_man));
        //TrapUtil.BaitException.Add(Cards.graveyard, new List<string>() {});
        TrapUtil.BaitException.Add(Cards.jungle, new List<string>() {Cards.giant_snail, Cards.snake});
        TrapUtil.BaitException.Add(Cards.mountain, new List<string>() {Cards.bear, Cards.ogre});
        TrapUtil.BaitException.Add(Cards.old_village, new List<string>() {Cards.orc_wizard});
        TrapUtil.BaitException.Add(Cards.plains, new List<string>() {Cards.giant_snail, Cards.snake});
        TrapUtil.BaitException.Add(Cards.spring, new List<string>() {Cards.mosquito, Cards.seagull, Cards.merman});
        TrapUtil.BaitException.Add(Cards.well, new List<string>() {Cards.mosquito, Cards.seagull, Cards.merman});
      }

      public static string IdeaName(string name)
      {
        return("Idea: " + name);
      }

      public static bool CheckHarvestable(ICardId mobId, Harvestable harves)
      {
        var loader = WorldManager.instance.GameDataLoader;
        if (mobId.Id == TrapUtil.DefaultMob)
          return true;
        if (TrapUtil.BaitException.ContainsKey(harves.Id))
        {
          if(TrapUtil.BaitException[harves.Id].Contains(mobId.Id))
          {
            return true;
          }
        }
        //check if the card is in the base CardBag
        foreach (CardChance c in harves.MyCardBag.Chances)
        {
          if (c.IsEnemy)
          {
            SetCardBagType setCardBagForEnemyCardBag = loader.GetSetCardBagForEnemyCardBag(c.EnemyBag);
            List<CardChance> chancesForSetCardBag = CardBag.GetChancesForSetCardBag(loader, setCardBagForEnemyCardBag);
            chancesForSetCardBag.RemoveAll((CardChance x) => ((loader.GetCardFromId(x.Id) as Combatable).ProcessedCombatStats.CombatLevel > c.Strength) ? true : false);
            if (chancesForSetCardBag.Exists(chance => CardBag.CardChanceToIds(chance, loader).Contains(mobId.Id)))
            {
              return true;
            }
          }
          else if (c.Id == mobId.Id)
          {
            return true;
          }
        }
        return false;
      }
    }
}
