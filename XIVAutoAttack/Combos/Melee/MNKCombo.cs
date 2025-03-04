using Dalamud.Game.ClientState.JobGauge.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using XIVAutoAttack;
using XIVAutoAttack.Combos;

namespace XIVAutoAttack.Combos.Melee;

internal class MNKCombo : CustomComboJob<MNKGauge>
{
    internal override uint JobID => 20;
    protected override bool ShouldSayout => true;

    internal struct Actions
    {
        public static readonly BaseAction
            //双龙脚
            DragonKick = new BaseAction(74)
            {
                BuffsProvide = new ushort[] { ObjectStatus.LeadenFist },
            },

            //连击
            Bootshine = new BaseAction(53),

            //破坏神冲 aoe
            ArmoftheDestroyer = new BaseAction(62),

            //双掌打 伤害提高
            TwinSnakes = new BaseAction(61),

            //正拳
            TrueStrike = new BaseAction(54),

            //四面脚 aoe
            FourpointFury = new BaseAction(16473),

            //破碎拳
            Demolish = new BaseAction(66)
            {
                EnermyLocation = EnemyLocation.Back,
            },

            //崩拳
            SnapPunch = new BaseAction(56)
            {
                EnermyLocation = EnemyLocation.Side,
            },

            //地烈劲 aoe
            Rockbreaker = new BaseAction(70),

            //斗气
            Meditation = new BaseAction(3546),

            //铁山靠
            SteelPeak = new BaseAction(25761)
            {
                OtherCheck = b => TargetHelper.InBattle,
            },

            //空鸣拳
            HowlingFist = new BaseAction(25763)
            {
                OtherCheck = b => TargetHelper.InBattle,
            },

            //义结金兰
            Brotherhood = new BaseAction(7396, true),

            //红莲极意 提高dps
            RiddleofFire = new BaseAction(7395),

            //突进技能
            Thunderclap = new BaseAction(25762, shouldEndSpecial: true)
            {
                ChoiceFriend = BaseAction.FindMoveTarget,
            },

            //真言
            Mantra = new BaseAction(65, true),

            //震脚
            PerfectBalance = new BaseAction(69)
            {
                OtherCheck = b => BaseAction.HaveStatusSelfFromSelf(ObjectStatus.RaptorForm) && TargetHelper.InBattle,
            },

            //苍气炮 阴
            ElixirField = new BaseAction(3545),

            //爆裂脚 阳
            FlintStrike = new BaseAction(25882),
            //凤凰舞
            RisingPhoenix = new BaseAction(25768),


            //斗魂旋风脚 阴阳
            TornadoKick = new BaseAction(3543),
            PhantomRush = new BaseAction(25769),

            //演武
            FormShift = new BaseAction(4262)
            {
                BuffsProvide = new ushort[] { ObjectStatus.FormlessFist, ObjectStatus.PerfectBalance },
            },

            //金刚极意 盾
            RiddleofEarth = new BaseAction(7394, shouldEndSpecial: true),

            //疾风极意
            RiddleofWind = new BaseAction(25766);
    }

    internal override SortedList<DescType, string> Description => new SortedList<DescType, string>()
    {
        {DescType.范围治疗, $"{Actions.Mantra.Action.Name}"},
        {DescType.单体防御, $"{Actions.RiddleofEarth.Action.Name}"},
        {DescType.移动, $"{Actions.Thunderclap.Action.Name}，目标为面向夹角小于30°内最远目标。"},
    };

    private protected override bool BreakAbility(byte abilityRemain, out IAction act)
    {
        if (Actions.RiddleofFire.ShouldUseAction(out act)) return true;
        if (Actions.Brotherhood.ShouldUseAction(out act)) return true;
        return false;
    }

    private protected override bool HealAreaAbility(byte abilityRemain, out IAction act)
    {
        if (Actions.Mantra.ShouldUseAction(out act)) return true;
        return false;
    }


    private protected override bool DefenceSingleAbility(byte abilityRemain, out IAction act)
    {
        if (Actions.RiddleofEarth.ShouldUseAction(out act, emptyOrSkipCombo: true)) return true;
        return false;
    }

    private protected override bool DefenceAreaAbility(byte abilityRemain, out IAction act)
    {
        if (GeneralActions.Feint.ShouldUseAction(out act)) return true;
        return false;
    }

    private protected override bool MoveAbility(byte abilityRemain, out IAction act)
    {
        if (Actions.Thunderclap.ShouldUseAction(out act, emptyOrSkipCombo: true)) return true;
        return false;
    }


    private bool OpoOpoForm(out IAction act)
    {
        if (Actions.ArmoftheDestroyer.ShouldUseAction(out act)) return true;
        if (Actions.DragonKick.ShouldUseAction(out act)) return true;
        if (Actions.Bootshine.ShouldUseAction(out act)) return true;
        return false;
    }


    private bool RaptorForm(out IAction act)
    {
        if (Actions.FourpointFury.ShouldUseAction(out act)) return true;

        //确认Buff大于6s
        var times = BaseAction.FindStatusSelfFromSelf(ObjectStatus.DisciplinedFist);
        if ((times.Length == 0 || times[0] < 4 + WeaponRemain) && Actions.TwinSnakes.ShouldUseAction(out act)) return true;

        if (Actions.TrueStrike.ShouldUseAction(out act)) return true;
        return false;
    }

    private bool CoerlForm(out IAction act)
    {
        if (Actions.Rockbreaker.ShouldUseAction(out act)) return true;
        if (Actions.Demolish.ShouldUseAction(out act))
        {
            var times = BaseAction.FindStatusFromSelf(Actions.Demolish.Target, ObjectStatus.Demolish);
            if (times.Length == 0 || times[0] < 4 + WeaponRemain || Math.Abs(times[0] - 4 + WeaponRemain) < 0.1) return true;
        }
        if (Actions.SnapPunch.ShouldUseAction(out act)) return true;
        return false;
    }

    private bool LunarNadi(out IAction act)
    {
        if (OpoOpoForm(out act)) return true;
        return false;
    }

    private bool SolarNadi(out IAction act)
    {
        if (!JobGauge.BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.RAPTOR))
        {
            if (RaptorForm(out act)) return true;
        }
        else if (!JobGauge.BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.OPOOPO))
        {
            if (OpoOpoForm(out act)) return true;
        }
        else
        {
            if (CoerlForm(out act)) return true;
        }

        return false;
    }

    private protected override bool GeneralGCD(uint lastComboActionID, out IAction act)
    {
        bool havesolar = (JobGauge.Nadi & Dalamud.Game.ClientState.JobGauge.Enums.Nadi.SOLAR) != 0;
        bool havelunar = (JobGauge.Nadi & Dalamud.Game.ClientState.JobGauge.Enums.Nadi.LUNAR) != 0;

        //满了的话，放三个大招
        if (!JobGauge.BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.NONE))
        {
            if (havesolar && havelunar)
            {
                if (Actions.PhantomRush.ShouldUseAction(out act, mustUse: true)) return true;
                if (Actions.TornadoKick.ShouldUseAction(out act, mustUse: true)) return true;
            }
            if (JobGauge.BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.RAPTOR))
            {
                if (Actions.RisingPhoenix.ShouldUseAction(out act, mustUse: true)) return true;
                if (Actions.FlintStrike.ShouldUseAction(out act, mustUse: true)) return true;
            }
            else
            {
                if (Actions.ElixirField.ShouldUseAction(out act, mustUse: true)) return true;
            }
        }
        //有震脚就阴阳
        else if (BaseAction.HaveStatusSelfFromSelf(ObjectStatus.PerfectBalance))
        {
            if (havesolar && LunarNadi(out act)) return true;
            if (SolarNadi(out act)) return true;
        }

        if (BaseAction.HaveStatusSelfFromSelf(ObjectStatus.CoerlForm))
        {
            if (CoerlForm(out act)) return true;
        }
        else if (BaseAction.HaveStatusSelfFromSelf(ObjectStatus.RaptorForm))
        {
            if (RaptorForm(out act)) return true;
        }
        else
        {
            if (OpoOpoForm(out act)) return true;
        }

        if (IconReplacer.Move && MoveAbility(1, out act)) return true;
        if (JobGauge.Chakra < 5 && Actions.Meditation.ShouldUseAction(out act)) return true;
        if (Actions.FormShift.ShouldUseAction(out act)) return true;

        return false;
    }

    private protected override bool ForAttachAbility(byte abilityRemain, out IAction act)
    {
        //震脚
        if (JobGauge.BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.NONE))
        {
            //有阳斗气
            if ((JobGauge.Nadi & Dalamud.Game.ClientState.JobGauge.Enums.Nadi.SOLAR) != 0)
            {
                //两种Buff都在6s以上
                var dis = BaseAction.FindStatusSelfFromSelf(ObjectStatus.DisciplinedFist);
                Actions.Demolish.ShouldUseAction(out _);
                var demo = BaseAction.FindStatusFromSelf(Actions.Demolish.Target, ObjectStatus.Demolish);
                if (dis.Length != 0 && dis[0] > 6 && (demo.Length != 0 && demo[0] > 6 || !Actions.PerfectBalance.IsCoolDown))
                {
                    if (Actions.PerfectBalance.ShouldUseAction(out act, emptyOrSkipCombo: true)) return true;
                }
            }
            else
            {
                if (Actions.PerfectBalance.ShouldUseAction(out act, emptyOrSkipCombo: true)) return true;
            }

        }

        if (Actions.RiddleofWind.ShouldUseAction(out act)) return true;

        if (JobGauge.Chakra == 5)
        {
            if (Actions.HowlingFist.ShouldUseAction(out act)) return true;
            if (Actions.SteelPeak.ShouldUseAction(out act)) return true;
            if (Actions.HowlingFist.ShouldUseAction(out act, mustUse: true)) return true;
        }

        return false;
    }


}
