﻿using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActionCategory = Lumina.Excel.GeneratedSheets.ActionCategory;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace XIVComboPlus.Combos
{
    internal class BaseAction
    {
        private Action _action;
        private uint _actionType => _action.ActionCategory.Value.RowId;
        internal byte Level => _action.ClassJobLevel;
        internal uint ActionID => _action.RowId;
        private bool IsAbility => _actionType == 4;
        private bool IsSpell => _actionType == 2;
        private bool IsWeaponskill => _actionType == 4;
        internal short CastTime => (short)(_action.Cast100ms * 100);
        internal virtual uint MPNeed
        {
            get
            {
                if(_action.PrimaryCostType == 3 || _action.PrimaryCostType == 4)
                {
                    return _action.PrimaryCostValue * 100u;
                }
                else if(_action.SecondaryCostType == 3 || _action.SecondaryCostType == 4)
                {
                    return _action.SecondaryCostValue * 100u;
                }
                return 0;
            }
        }
        /// <summary>
        /// 如果之前是这些ID，那么就不会执行。
        /// </summary>
        internal uint[] OtherIDs { private get; set; } = null;
        /// <summary>
        /// 是不是需要加上所有的Debuff
        /// </summary>
        internal bool NeedAllDebuffs { private get; set; } = false;
        /// <summary>
        /// 给敌人造成的Debuff,如果有这些Debuff，那么不会执行。
        /// </summary>
        internal ushort[] Debuffs { get; set; } = null;
        /// <summary>
        /// 使用了这个技能会得到的Buff，如果有这些Buff中的一种，那么就不会执行。 
        /// </summary>
        internal ushort[] BuffsProvide { get; set; } = null;

        /// <summary>
        /// 使用这个技能需要的前置Buff，有任何一个就好。
        /// </summary>
        internal ushort[] BuffsNeed { get; set; } = null;

        /// <summary>
        /// 使用这个技能不能有的Buff。
        /// </summary>
        internal ushort[] BuffsCantHave { get; set; } = null;

        /// <summary>
        /// 如果有一些别的需要判断的，可以写在这里。True表示可以使用这个技能。
        /// </summary>
        internal Func<bool> OtherCheck { private get; set; } = null;

        internal BaseAction(uint actionID)
        {
            _action = Service.DataManager.GetExcelSheet<Action>().GetRow(actionID);
        }

        public bool TryUseAction(byte level, out uint action, uint lastAct = 0, bool mustUse = false)
        {
            action = ActionID;

            //等级不够。
            if (level < this.Level) return false;

            //MP不够
            if (Service.ClientState.LocalPlayer.CurrentMp < this.MPNeed) return false;

            //没有前置Buff
            if(BuffsNeed != null)
            {
                bool findFuff = false;
                foreach (var buff in BuffsNeed)
                {
                    if (HaveStatus(FindStatusSelfFromSelf(buff)))
                    {
                        findFuff = true;
                        break;
                    }
                }
                if(!findFuff) return false;
            }

            //如果有不能拥有的Buff的话，就返回。
            if(BuffsCantHave != null)
            {
                foreach (var buff in BuffsCantHave)
                {
                    if (HaveStatus(FindStatusSelfFromSelf(buff)))
                    {
                        return false;
                    }
                }
            }

            //如果是能力技能，而且没法释放。
            if (IsAbility)
            {
                int charge = _action.MaxCharges;
                var CoolDown = Service.IconReplacer.GetCooldown(ActionID);
                if (charge < 2 && CoolDown.IsCooldown) return false;

                if (CoolDown.CooldownElapsed / CoolDown.CooldownTotal < 1f / charge) return false;
            }

            //已有提供的Buff的任何一种
            if (BuffsProvide != null)
            {
                foreach (var buff in BuffsProvide)
                {
                    if (HaveStatus(FindStatusSelfFromSelf(buff))) return false;
                }
            }

            //如果必须要用，那么以下的条件就不用判断了。
            if (mustUse) return true;

            //如果有输入上次的数据，那么上次不能是上述的ID。
            if (OtherIDs != null)
            {
                foreach (var id in OtherIDs)
                {
                    if (lastAct == id) return false;
                }
            }

            //如果有Combo，有LastAction，而且上次不是连击，那就不触发。
            uint comboAction = _action.ActionCombo.Row;
            if (comboAction != lastAct) return false;



            //敌方已有充足的Debuff
            if (Debuffs != null)
            {
                if (NeedAllDebuffs)
                {
                    bool haveAll = true;
                    foreach (var debuff in Debuffs)
                    {
                        if (!EnoughStatus(FindStatusTargetFromSelf(debuff)))
                        {
                            haveAll = false;
                            break;
                        }
                    }
                    if (haveAll) return false;
                }
                else
                {
                    foreach (var debuff in Debuffs)
                    {
                        if (EnoughStatus(FindStatusTargetFromSelf(debuff))) return false;
                    }
                }
            }

            //如果是能力技能，还没填满。
            if (IsAbility && Service.IconReplacer.GetCooldown(ActionID).IsCooldown) return false;

            //如果是个法术，并且还在移动，也没有即刻相关的技能。
            if (TargetHelper.IsMoving && IsSpell)
            {
                bool haveSwift = false;
                foreach (var buff in CustomCombo.GeneralActions.Swiftcast.BuffsProvide)
                {
                    if (HaveStatus(FindStatusSelfFromSelf(buff)))
                    {
                        haveSwift = true;
                        break;
                    }
                }
                if(!haveSwift ) return false;
            }

            //用于自定义的要求没达到
            if (OtherCheck != null && !OtherCheck()) return false;

            //如果是个范围，并且人数不够的话，就算了。
            if(!TargetHelper.ShoudUseAction(_action)) return false;

            return true;
        }

        internal static bool EnoughStatus(Status status)
        {
            return StatusRemainTime(status) > 3f; ;
        }

        internal static bool HaveStatus(Status status)
        {
            return StatusRemainTime(status) != 0f;
        }
        internal static float StatusRemainTime(Status status)
        {
            return status?.RemainingTime ?? 0f;
        }

        /// <summary>
        /// 找到任何对象附加到自己敌人的状态。
        /// </summary>
        /// <param name="effectID"></param>
        /// <returns></returns>
        internal static Status FindStatusTarget(ushort effectID)
        {
            return FindStatus(effectID, Service.TargetManager.Target, null);
        }

        /// <summary>
        /// 找到任何对象附加到自己身上的状态。
        /// </summary>
        /// <param name="effectID"></param>
        /// <returns></returns>
        internal static Status FindStatusSelf(ushort effectID)
        {
            return FindStatus(effectID, Service.ClientState.LocalPlayer, null);
        }

        /// <summary>
        /// 找到玩家附加到敌人身上的状态。
        /// </summary>
        /// <param name="effectID"></param>
        /// <returns></returns>
        internal static Status FindStatusTargetFromSelf(ushort effectID)
        {
            GameObject currentTarget = Service.TargetManager.Target;
            PlayerCharacter localPlayer = Service.ClientState.LocalPlayer;
            return FindStatus(effectID, currentTarget, localPlayer != null ? new uint?(localPlayer.ObjectId) : null);
        }

        /// <summary>
        /// 找到自己附加到自己身上的状态。
        /// </summary>
        /// <param name="effectID"></param>
        /// <returns></returns>
        internal static Status FindStatusSelfFromSelf(ushort effectID)
        {
            PlayerCharacter localPlayer = Service.ClientState.LocalPlayer,
                            localPlayer2 = Service.ClientState.LocalPlayer;
            return FindStatus(effectID, localPlayer, localPlayer2 != null ? new uint?(localPlayer2.ObjectId) : null);
        }

        private static Status FindStatus(ushort effectID, GameObject obj, uint? sourceID)
        {
            if (obj == null)
            {
                return null;
            }
            BattleChara val = (BattleChara)obj;
            if (val == null)
            {
                return null;
            }
            foreach (Status status in val.StatusList)
            {
                if (status.StatusId == effectID && (!sourceID.HasValue || status.SourceID == 0 || status.SourceID == 3758096384u || status.SourceID == sourceID))
                {
                    return status;
                }
            }
            return null;
        }
    }
}
