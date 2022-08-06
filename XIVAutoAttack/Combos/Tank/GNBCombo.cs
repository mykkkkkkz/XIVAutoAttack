using Dalamud.Game.ClientState.JobGauge.Types;
using System.Collections.Generic;
using System.Numerics;
namespace XIVAutoAttack.Combos.Tank;

internal class GNBCombo : CustomComboJob<GNBGauge>
{
    internal override uint JobID => 37;
    internal override bool HaveShield => BaseAction.HaveStatusSelfFromSelf(ObjectStatus.RoyalGuard);
    private protected override BaseAction Shield => Actions.RoyalGuard;

    protected override bool CanHealSingleSpell => false;
    protected override bool CanHealAreaSpell => false;

    internal struct Actions
    {
        public static readonly BaseAction
            //王室亲卫
            RoyalGuard = new BaseAction(16142, shouldEndSpecial: true),

            //利刃斩
            KeenEdge = new BaseAction(16137),

            //无情
            NoMercy = new BaseAction(16138),

            //残暴弹
            BrutalShell = new BaseAction(16139),

            //伪装
            Camouflage = new BaseAction(16140)
            {
                BuffsProvide = GeneralActions.Rampart.BuffsProvide,
            },

            //恶魔切
            DemonSlice = new BaseAction(16141),

            //闪雷弹
            LightningShot = new BaseAction(16143),

            //危险领域
            DangerZone = new BaseAction(16144),

            //迅连斩
            SolidBarrel = new BaseAction(16145),

            //爆发击
            BurstStrike = new BaseAction(16162),

            //星云
            Nebula = new BaseAction(16148)
            {
                BuffsProvide = GeneralActions.Rampart.BuffsProvide,
            },

            //恶魔杀
            DemonSlaughter = new BaseAction(16149),

            //极光
            Aurora = new BaseAction(16151, true)
            {
                BuffsProvide = new ushort[] { ObjectStatus.Aurora },
            },

            //超火流星
            Superbolide = new BaseAction(16152)
            {
                OtherCheck = b => (float)Service.ClientState.LocalPlayer.CurrentHp / Service.ClientState.LocalPlayer.MaxHp < Service.Configuration.HealthForDyingTank,
            },

            //音速破
            SonicBreak = new BaseAction(16153),

            //粗分斩
            RoughDivide = new BaseAction(16154, shouldEndSpecial: true),

            //烈牙
            GnashingFang = new BaseAction(16146),

            //弓形冲波
            BowShock = new BaseAction(16159),

            //光之心
            HeartofLight = new BaseAction(16160, true),

            //石之心
            HeartofStone = new BaseAction(16161, true)
            {
                BuffsProvide = GeneralActions.Rampart.BuffsProvide,
            },

            //命运之环
            FatedCircle = new BaseAction(16163),

            //血壤
            Bloodfest = new BaseAction(16164)
            {
                OtherCheck = b => JobGauge.Ammo == 0,
            },

            //倍攻
            DoubleDown = new BaseAction(25760),

            //猛兽爪
            SavageClaw = new BaseAction(16147),

            //凶禽爪
            WickedTalon = new BaseAction(16150),

            //撕喉
            JugularRip = new BaseAction(16156)
            {
                OtherCheck = b => Service.IconReplacer.OriginalHook(16155) == JugularRip.ID,
            },

            //裂膛
            AbdomenTear = new BaseAction(16157)
            {
                OtherCheck = b => Service.IconReplacer.OriginalHook(16155) == AbdomenTear.ID,
            },

            //穿目
            EyeGouge = new BaseAction(16158)
            {
                OtherCheck = b => Service.IconReplacer.OriginalHook(16155) == EyeGouge.ID,
            },

            //超高速
            Hypervelocity = new BaseAction(25759)
            {
                OtherCheck = b => Service.IconReplacer.OriginalHook(16155) == Hypervelocity.ID,
            };
    }
    internal override SortedList<DescType, string> Description => new SortedList<DescType, string>()
    {
        {DescType.单体治疗, $"{Actions.Aurora.Action.Name}"},
        {DescType.范围防御, $"{Actions.HeartofLight.Action.Name}"},
        {DescType.单体防御, $"{Actions.HeartofStone.Action.Name}, {Actions.Nebula.Action.Name}, {Actions.Camouflage.Action.Name}"},
        {DescType.移动, $"{Actions.RoughDivide.Action.Name}"},
    };
    private protected override bool GeneralGCD(uint lastComboActionID, out IAction act)
    {
        //使用晶囊
        bool useAmmo = JobGauge.Ammo > (Service.ClientState.LocalPlayer.Level > Actions.DoubleDown.Level ? 2 : 0);


        uint remap = Service.IconReplacer.OriginalHook(Actions.GnashingFang.ID);
        if (remap == Actions.WickedTalon.ID && Actions.WickedTalon.ShouldUseAction(out act)) return true;
        if (remap == Actions.SavageClaw.ID && Actions.SavageClaw.ShouldUseAction(out act)) return true;

        //AOE
        if (useAmmo)
        {
            if (Actions.DoubleDown.ShouldUseAction(out act, mustUse: true)) return true;
            if (Actions.FatedCircle.ShouldUseAction(out act)) return true;
        }
        if ( Actions.DemonSlaughter.ShouldUseAction(out act, lastComboActionID)) return true;
        if ( Actions.DemonSlice.ShouldUseAction(out act, lastComboActionID)) return true;

        //单体
        if (useAmmo)
        {
            if (Actions.GnashingFang.ShouldUseAction(out act)) return true;
            if (Actions.BurstStrike.ShouldUseAction(out act)) return true;
        }
        if (Actions.SonicBreak.ShouldUseAction(out act)) return true;

        //单体三连
        if (Actions.SolidBarrel.ShouldUseAction(out act, lastComboActionID)) return true;
        if (Actions.BrutalShell.ShouldUseAction(out act, lastComboActionID)) return true;
        if (Actions.KeenEdge.ShouldUseAction(out act, lastComboActionID)) return true;

        if (IconReplacer.Move && MoveAbility(1, out act)) return true;
        if (Actions.LightningShot.ShouldUseAction(out act)) return true;

        return false;
    }

    private protected override bool EmergercyAbility(byte abilityRemain, IAction nextGCD, out IAction act)
    {
        //神圣领域 如果谢不够了。
        if (Actions.Superbolide.ShouldUseAction(out act)) return true;
        return false;
    }

    private protected override bool ForAttachAbility(byte abilityRemain, out IAction act)
    {
        if (Actions.JugularRip.ShouldUseAction(out act)) return true;
        if (Actions.AbdomenTear.ShouldUseAction(out act)) return true;
        if (Actions.EyeGouge.ShouldUseAction(out act)) return true;
        if (Actions.Hypervelocity.ShouldUseAction(out act)) return true;


        if (Actions.NoMercy.ShouldUseAction(out act)) return true;
        if (Actions.Bloodfest.ShouldUseAction(out act)) return true;
        if (Actions.BowShock.ShouldUseAction(out act, mustUse: true)) return true;
        if (Actions.DangerZone.ShouldUseAction(out act)) return true;

        //搞搞攻击
        if (Actions.RoughDivide.ShouldUseAction(out act) && !IsMoving)
        {
            if (BaseAction.DistanceToPlayer(Actions.RoughDivide.Target) < 1)
            {
                return true;
            }
        }
        return false;
    }

    private protected override bool DefenceAreaAbility(byte abilityRemain, out IAction act)
    {
        if (Actions.HeartofLight.ShouldUseAction(out act, emptyOrSkipCombo: true)) return true;
        return false;
    }

    private protected override bool MoveAbility(byte abilityRemain, out IAction act)
    {
        //突进
        if (Actions.RoughDivide.ShouldUseAction(out act, emptyOrSkipCombo: true)) return true;
        return false;
    }
    private protected override bool DefenceSingleAbility(byte abilityRemain, out IAction act)
    {
        if (abilityRemain == 1)
        {

            //减伤10%）
            if (Actions.HeartofStone.ShouldUseAction(out act)) return true;

            //星云（减伤30%）
            if (Actions.Nebula.ShouldUseAction(out act)) return true;

            //铁壁（减伤20%）
            if (GeneralActions.Rampart.ShouldUseAction(out act)) return true;

            //伪装（减伤10%）
            if (Actions.Camouflage.ShouldUseAction(out act)) return true;

            //降低攻击
            //雪仇
            if (GeneralActions.Reprisal.ShouldUseAction(out act)) return true;
        }

        act = null;
        return false;
    }

    private protected override bool HealSingleAbility(byte abilityRemain, out IAction act)
    {
        if (Actions.Aurora.ShouldUseAction(out act, emptyOrSkipCombo: true) && abilityRemain == 1) return true;

        return false;
    }
}
