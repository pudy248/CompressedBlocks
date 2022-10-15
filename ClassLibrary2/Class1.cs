using BrilliantSkies.Core.Constants;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Ftd.Constructs.Modules.Main.Power;
using BrilliantSkies.Modding;
using BrilliantSkies.Ui.Special.InfoStore;
using BrilliantSkies.Ui.Tips;
using HarmonyLib;
using pudy248.CoreLib;
using System;
using System.Collections.Generic;

#pragma warning disable CS0618

namespace pudy248.CompressedBlocks
{
	public class Plugin : GamePlugin
	{
		public static Config cfg;
		public string name
		{
			get { return "CompressedBlocks"; }
		}
		public Version version
		{
			get { return new Version(1, 1, 0, 0); }
		}
		public void OnLoad()
		{
			ConfigDefinition def = new ConfigDefinition("com.pudy248.CompressedBlocks", "CompressedBlocks.cfg");

			def.AddEntry("Level1CompressionScale", "The stat multiplier of Level 1 compressed blocks", "System.Single", 2f);
			def.AddEntry("Level1CompressionCost", "The material cost multiplier of Level 1 compressed blocks", "System.Single", 2.5f);

			def.AddEntry("Level2CompressionScale", "The stat multiplier of Level 2 compressed blocks", "System.Single", 5f);
			def.AddEntry("Level2CompressionCost", "The material cost multiplier of Level 2 compressed blocks", "System.Single", 8f);

			def.AddEntry("Level3CompressionScale", "The stat multiplier of Level 3 compressed blocks", "System.Single", 10f);
			def.AddEntry("Level3CompressionCost", "The material cost multiplier of Level 3 compressed blocks", "System.Single", 20f);

			ConfigHelper.CreateFile(def);
			cfg = ConfigHelper.AppendToExisting(ConfigHelper.LoadFile("CompressedBlocks.cfg"), def);

			CallRedirector.GlobalRedirect(AccessTools.Method(typeof(ShellRackPhysicalContainer), "IsThereRoomForShell", new Type[] {typeof(int), typeof(float)}),
				AccessTools.Method(typeof(CompressedShellRackPhysicalContainer), "IsThereRoomForShell", new Type[] {typeof(ShellRackPhysicalContainer), typeof(int), typeof(float)}));

			CallRedirector.GlobalRedirect(AccessTools.Method(typeof(ShellRackPhysicalContainer), "MaximumRoundCountForDiameter", new Type[] { typeof(int) }),
				AccessTools.Method(typeof(CompressedShellRackPhysicalContainer), "MaximumRoundCountForDiameter", new Type[] { typeof(ShellRackPhysicalContainer), typeof(int) }));


			AdvLogger.LogInfo("Compressed Blocks loaded");
		}
		public void OnSave() { }
	}
	public class CompressedRTG : RTG
	{
		public int CompressionLevel;
		public float scaleFactor;
		static List<Guid> overriddenGuids = new List<Guid>();

		public void TryItemOverride()
		{
			CompressionLevel = item.Code.Variables.GetInt("CompressionLevel");
			scaleFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionScale"];
			float costFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionCost"];
			if (!overriddenGuids.Contains(item.ComponentId.Guid))
			{
				this.item.Weight *= scaleFactor;
				this.item.Health *= scaleFactor;
				this.item.Cost.Material *= costFactor;

				overriddenGuids.Add(item.ComponentId.Guid);
			}
		}

		public override void ItemSet()
		{
			TryItemOverride();
			base.ItemSet();
		}

		public override void AppendToolTip(ProTip tip)
		{
			TryItemOverride();
			tip.SetSpecial_Name(RTG._locFile.Get("SpecialName", "Compressed Radioisotope Thermoelectric Generator (RTG) - x" + scaleFactor.ToString(), true), RTG._locFile.Get("SpecialDescription", "RTGs generate endless streams of energy for batteries", true));
			tip.InfoOnly = true;
			tip.Add(Position.Middle, RTG._locFile.Format("Return_CreatesEnergyPer", "Creates <<{0} energy per second>> and puts it straight into batteries", new object[]
			{
			ConstructablePowerManager._baseEnergyPerVolumeOfRtgPerSecond * (float)base.item.SizeInfo.ArrayPositionsUsed * scaleFactor
			}));
		}

		public override BlockTechInfo GetTechInfo()
		{
			TryItemOverride();
			return new BlockTechInfo().AddSpec(RTG._locFile.Get("TechInfo_EnergyGeneratedPer", "Energy generated per second", true), ConstructablePowerManager._baseEnergyPerVolumeOfRtgPerSecond * (float)base.item.SizeInfo.ArrayPositionsUsed * scaleFactor).AddStatement(RTG._locFile.Format("TechInfo_IrDetectionRange", "Adds {0}{1} to body temperature of the vehicle. This will have a small impact on IR detection range from all angles.", new object[]
			{
			this.HeatChange,
			"°C/m³"
			}));
		}

		public override void StateChanged(IBlockStateChange change)
		{
			TryItemOverride();
			base.StateChanged(change);
			bool isAvailableToConstruct = change.IsAvailableToConstruct;
			if (isAvailableToConstruct)
			{
				base.MainConstruct.PowerUsageCreationAndFuelRestricted.RtgVolume += (float)base.item.SizeInfo.ArrayPositionsUsed * scaleFactor;
				base.MainConstruct.iBlockTypeStorage.RTGStore.Add(this);
				base.MainConstruct.HotObjectsRestricted.AddASimpleSourceOfBodyHeat((float)(base.item.SizeInfo.ArrayPositionsUsed * 10 * scaleFactor));
			}
			else
			{
				bool isLostToConstructOrConstructLost = change.IsLostToConstructOrConstructLost;
				if (isLostToConstructOrConstructLost)
				{
					base.MainConstruct.PowerUsageCreationAndFuelRestricted.RtgVolume -= (float)base.item.SizeInfo.ArrayPositionsUsed * scaleFactor;
					base.MainConstruct.iBlockTypeStorage.RTGStore.Remove(this);
					base.MainConstruct.HotObjectsRestricted.RemoveASimpleSourceofBodyHeat((float)(base.item.SizeInfo.ArrayPositionsUsed * 10 * scaleFactor));
				}
			}
		}
	}

	public class CompressedCannonExplosiveWarhead : CannonExplosiveWarhead
	{
		public int CompressionLevel;
		public float scaleFactor;
		static List<Guid> overriddenGuids = new List<Guid>();

		public void TryItemOverride()
		{
			CompressionLevel = item.Code.Variables.GetInt("CompressionLevel");
			scaleFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionScale"];
			float costFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionCost"];
			if (!overriddenGuids.Contains(item.ComponentId.Guid))
			{
				this.item.Weight *= scaleFactor;
				this.item.Health *= scaleFactor;
				this.item.Cost.Material *= costFactor;

				overriddenGuids.Add(item.ComponentId.Guid);
			}
		}

		public override void ItemSet()
		{
			TryItemOverride();
			base.ItemSet();
		}
		public override void SpecialUpdateConnectedTypeInfo(CramConnectedTypeInfo ConnectedTypeInfo)
		{
			TryItemOverride();
			ConnectedTypeInfo.expMaterials += (int)scaleFactor;
		}
		public override void AppendSpecial(ProTip tip)
		{
			tip.SetSpecial_Name(CannonExplosiveWarhead._locFile.Get("SpecialName", "Explosive Pellets x" + scaleFactor.ToString(), true), CannonExplosiveWarhead._locFile.Get("SpecialDescription", "Add explosive damage and radius to shells", true));
		}
	}
	public class CompressedCannonEmpWarhead : CannonEmpWarhead
	{
		public int CompressionLevel;
		public float scaleFactor;
		static List<Guid> overriddenGuids = new List<Guid>();

		public void TryItemOverride()
		{
			CompressionLevel = item.Code.Variables.GetInt("CompressionLevel");
			scaleFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionScale"];
			float costFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionCost"];
			if (!overriddenGuids.Contains(item.ComponentId.Guid))
			{
				this.item.Weight *= scaleFactor;
				this.item.Health *= scaleFactor;
				this.item.Cost.Material *= costFactor;

				overriddenGuids.Add(item.ComponentId.Guid);
			}
		}

		public override void ItemSet()
		{
			TryItemOverride();
			base.ItemSet();
		}
		public override void SpecialUpdateConnectedTypeInfo(CramConnectedTypeInfo ConnectedTypeInfo)
		{
			TryItemOverride();
			ConnectedTypeInfo.empMaterials += (int)scaleFactor;
		}
		public override void AppendSpecial(ProTip tip)
		{
			tip.SetSpecial_Name(CannonExplosiveWarhead._locFile.Get("SpecialName", "EMP Pellets x" + scaleFactor.ToString(), true), CannonExplosiveWarhead._locFile.Get("SpecialDescription", "Add EMP damage to shells", true));
		}
	}
	public class CompressedCannonFragWarhead : CannonFragWarhead
	{
		public int CompressionLevel;
		public float scaleFactor;
		static List<Guid> overriddenGuids = new List<Guid>();

		public void TryItemOverride()
		{
			CompressionLevel = item.Code.Variables.GetInt("CompressionLevel");
			scaleFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionScale"];
			float costFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionCost"];
			if (!overriddenGuids.Contains(item.ComponentId.Guid))
			{
				this.item.Weight *= scaleFactor;
				this.item.Health *= scaleFactor;
				this.item.Cost.Material *= costFactor;

				overriddenGuids.Add(item.ComponentId.Guid);
			}
		}

		public override void ItemSet()
		{
			TryItemOverride();
			base.ItemSet();
		}
		public override void SpecialUpdateConnectedTypeInfo(CramConnectedTypeInfo ConnectedTypeInfo)
		{
			TryItemOverride();
			ConnectedTypeInfo.fragMaterials += (int)scaleFactor;
		}
		public override void AppendSpecial(ProTip tip)
		{
			tip.SetSpecial_Name(CannonExplosiveWarhead._locFile.Get("SpecialName", "Frag Pellets x" + scaleFactor.ToString(), true), CannonExplosiveWarhead._locFile.Get("SpecialDescription", "Add fragment damage to shells", true));
		}
	}
	public class CompressedCannonArmourPiercingWarhead : CannonArmourPiercingWarhead
	{
		public int CompressionLevel;
		public float scaleFactor;
		static List<Guid> overriddenGuids = new List<Guid>();

		public void TryItemOverride()
		{
			CompressionLevel = item.Code.Variables.GetInt("CompressionLevel");
			scaleFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionScale"];
			float costFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionCost"];
			if (!overriddenGuids.Contains(item.ComponentId.Guid))
			{
				this.item.Weight *= scaleFactor;
				this.item.Health *= scaleFactor;
				this.item.Cost.Material *= costFactor;

				overriddenGuids.Add(item.ComponentId.Guid);
			}
		}

		public override void ItemSet()
		{
			TryItemOverride();
			base.ItemSet();
		}
		public override void SpecialUpdateConnectedTypeInfo(CramConnectedTypeInfo ConnectedTypeInfo)
		{
			TryItemOverride();
			ConnectedTypeInfo.hardenerMaterials += (int)scaleFactor;
		}
		public override void AppendSpecial(ProTip tip)
		{
			tip.SetSpecial_Name(CannonExplosiveWarhead._locFile.Get("SpecialName", "Armour Piercing Pellets x" + scaleFactor.ToString(), true), CannonExplosiveWarhead._locFile.Get("SpecialDescription", "Add kinetic damage and armour piercing to shells", true));
		}
	}
	public class CompressedCannonAmmoBox : CannonAmmoBox
	{
		public int CompressionLevel;
		public float scaleFactor;
		static List<Guid> overriddenGuids = new List<Guid>();

		public void TryItemOverride()
		{
			CompressionLevel = item.Code.Variables.GetInt("CompressionLevel");
			scaleFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionScale"];
			float costFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionCost"];
			if (!overriddenGuids.Contains(item.ComponentId.Guid))
			{
				this.item.Weight *= scaleFactor;
				this.item.Health *= scaleFactor;
				this.item.Cost.Material *= costFactor;

				overriddenGuids.Add(item.ComponentId.Guid);
			}
		}

		public override void ItemSet()
		{
			TryItemOverride();
			base.ItemSet();
		}
		public override void SpecialUpdateConnectedTypeInfo(CramConnectedTypeInfo ConnectedTypeInfo)
		{
			TryItemOverride();
			ConnectedTypeInfo.compactors += (int)scaleFactor;
		}
		public override void AppendSpecial(ProTip tip)
		{
			tip.SetSpecial_Name(CannonExplosiveWarhead._locFile.Get("SpecialName", "Payload compactor x" + scaleFactor.ToString(), true), CannonExplosiveWarhead._locFile.Get("SpecialDescription", "Increases the payload capacity of the cannon's shells.", true));
		}
	}

	public class CompressedLaserDestabiliser : LaserDestabiliser
	{
		public int CompressionLevel;
		public float scaleFactor;
		static List<Guid> overriddenGuids = new List<Guid>();

		public void TryItemOverride()
		{
			CompressionLevel = item.Code.Variables.GetInt("CompressionLevel");
			scaleFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionScale"];
			float costFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionCost"];
			if (!overriddenGuids.Contains(item.ComponentId.Guid))
			{
				this.item.Weight *= scaleFactor;
				this.item.Health *= scaleFactor;
				this.item.Cost.Material *= costFactor;

				overriddenGuids.Add(item.ComponentId.Guid);
			}
		}

		public override void ItemSet()
		{
			TryItemOverride();
			base.ItemSet();
		}

		public override void FeelerFlowDown(LaserFeeler feeler)
		{
			TryItemOverride();
			base.FeelerFlowDown(feeler);
			feeler.destabilisers += (int)scaleFactor - 1;
		}
		public override void AppendToolTip(ProTip tip)
		{
			base.AppendToolTip(tip);
			tip.SetSpecial_Name(LaserDestabiliser._locFile.Get("SpecialName", "Laser Destabiliser x" + scaleFactor.ToString(), true), LaserDestabiliser._locFile.Get("SpecialDescription", "Discharges energy from cavities quicker resulting in the same damage over a smaller window of time.  Connect to cavities or other connected cavity components." + this.GetConnectionText(), true));
		}
	}
	public class CompressedLaserFrequencyDoubler : LaserFrequencyDoubler
	{
		public int CompressionLevel;
		public float scaleFactor;
		static List<Guid> overriddenGuids = new List<Guid>();

		public void TryItemOverride()
		{
			CompressionLevel = item.Code.Variables.GetInt("CompressionLevel");
			scaleFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionScale"];
			float costFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionCost"];
			if (!overriddenGuids.Contains(item.ComponentId.Guid))
			{
				this.item.Weight *= scaleFactor;
				this.item.Health *= scaleFactor;
				this.item.Cost.Material *= costFactor;

				overriddenGuids.Add(item.ComponentId.Guid);
			}
		}

		public override void ItemSet()
		{
			TryItemOverride();
			base.ItemSet();
		}

		public override void FeelerFlowDown(LaserFeeler feeler)
		{
			TryItemOverride();
			base.FeelerFlowDown(feeler);
			feeler.frequencyDoublers += (int)scaleFactor - 1;
		}
		public override void AppendToolTip(ProTip tip)
		{
			base.AppendToolTip(tip);
			tip.SetSpecial_Name(LaserFrequencyDoubler._locFile.Get("SpecialName", "Laser Frequency Doubler x" + scaleFactor.ToString(), true), LaserFrequencyDoubler._locFile.Get("SpecialDescription", "Increases AP value of the laser.  Connect to couplers, cavities or other connected cavity components.", true));
		}
	}
	public class CompressedLaserPump : LaserPump
	{
		public int CompressionLevel;
		public float scaleFactor;
		static List<Guid> overriddenGuids = new List<Guid>();

		public void TryItemOverride()
		{
			CompressionLevel = item.Code.Variables.GetInt("CompressionLevel");
			scaleFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionScale"];
			float costFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionCost"];
			if (!overriddenGuids.Contains(item.ComponentId.Guid))
			{
				this.item.Weight *= scaleFactor;
				this.item.Health *= scaleFactor;
				this.item.Cost.Material *= costFactor;

				overriddenGuids.Add(item.ComponentId.Guid);
			}
		}

		public override void ItemSet()
		{
			TryItemOverride();
			base.ItemSet();
		}

		new public float CubicMeterPowerNeeded
		{
			get
			{
				return LaserConstants.PowerPerCavityEnergy * LaserConstants.EnergyPumpRatePerCubicMeter * ((int)scaleFactor - 1);
			}
		}

		public override void FeelerFlowDown(LaserFeeler feeler)
		{
			TryItemOverride();
			base.FeelerFlowDown(feeler);
			feeler.CubicMetresOfPumping += base.item.SizeInfo.ArrayPositionsUsed * (int)scaleFactor - 1;
		}
		public override void AppendToolTip(ProTip tip)
		{
			base.AppendToolTip(tip);
			tip.SetSpecial_Name(LaserPump._locFile.Get("SpecialName", "Laser Pump x" + scaleFactor.ToString(), true), LaserPump._locFile.Get("SpecialDescription", "Pumps energy in to the laser cavity.  Connects to the laser cavity." + this.GetConnectionText(), true));
		}
	}
	public class CompressedLaserCavity : LaserCavity
	{
		public int CompressionLevel;
		public float scaleFactor;
		static List<Guid> overriddenGuids = new List<Guid>();

		public void TryItemOverride()
		{
			CompressionLevel = item.Code.Variables.GetInt("CompressionLevel");
			scaleFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionScale"];
			float costFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionCost"];
			if (!overriddenGuids.Contains(item.ComponentId.Guid))
			{
				this.item.Weight *= scaleFactor;
				this.item.Health *= scaleFactor;
				this.item.Cost.Material *= costFactor;

				overriddenGuids.Add(item.ComponentId.Guid);
			}
		}

		public override void ItemSet()
		{
			TryItemOverride();
			base.ItemSet();
			this.energyPerCavity *= scaleFactor;
		}

		public override void FeelerFlowDown(LaserFeeler feeler)
		{
			TryItemOverride();
			base.FeelerFlowDown(feeler);
			feeler.energyCapacity += this.energyPerCavity;
		}
		public override void AppendToolTip(ProTip tip)
		{
			base.AppendToolTip(tip);
			tip.SetSpecial_Name(LaserCavity._locFile.Get("SpecialName", "Laser Cavity x" + scaleFactor.ToString(), true), LaserCavity._locFile.Get("SpecialDescription", "Where the beam is generated based on connected pumps, storage, destabilisers and frequency doublers.", true));
		}
	}

	public class CompressedParticleCannonPipe : ParticleCannonPipe
	{
		public int CompressionLevel;
		public float scaleFactor;
		static List<Guid> overriddenGuids = new List<Guid>();

		public void TryItemOverride()
		{
			CompressionLevel = item.Code.Variables.GetInt("CompressionLevel");
			scaleFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionScale"];
			float costFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionCost"];
			if (!overriddenGuids.Contains(item.ComponentId.Guid))
			{
				this.item.Weight *= scaleFactor;
				this.item.Health *= scaleFactor;
				this.item.Cost.Material *= costFactor;

				overriddenGuids.Add(item.ComponentId.Guid);
			}
		}

		public override void ItemSet()
		{
			TryItemOverride();
			base.ItemSet();
		}

		public override void FeelerFlowDown(ParticleCannonFeeler feeler)
		{
			TryItemOverride();
			this._armIndex = feeler.Node.Arms.CurrentArm.Index;
			base.Node.Arms.CurrentArm.Length += item.SizeInfo.ArrayPositionsUsed * (int)scaleFactor;
			base.Node.Arms.CurrentArm.LastPipe = this;
		}

		public override BlockTechInfo GetTechInfo()
		{
			TryItemOverride();
			BlockTechInfo blockTechInfo = new BlockTechInfo();
			blockTechInfo.AddSpec(ParticleCannonPipe._locFile.Get("TechInfo_EnergyUse", "Energy charging rate", true), string.Format("{0}/s", Rounding.R0(ParticleCannonConstants.PipeEnergyPerSec * scaleFactor * (float)base.item.SizeInfo.ArrayPositionsUsed)));
			return blockTechInfo;
		}
	}

	public class CompressedShellRackPhysicalContainer : ShellRackPhysicalContainer
	{
		public CompressedShellRackPhysicalContainer(AdvCannonAmmoClip ammoClip) : base(ammoClip)
		{

		}

		public static bool IsThereRoomForShell(ShellRackPhysicalContainer instance, int diameter, float length)
		{
			if (instance is CompressedShellRackPhysicalContainer cInstance)
			{
				CompressedAdvCannonAmmoClip clip = cInstance.AmmoClip as CompressedAdvCannonAmmoClip;
				if (clip == null) throw new Exception("A compressed shell rack container was added to an uncompressed clip!");
				double num = 1E-05;
				bool result;
				if ((double)length > (double)instance.LengthCapacity + num)
				{
					InfoStore.Add(ShellRackPhysicalContainer._locFile.Get("Error_ShellTooLong", "Shell is too long for the shell rack- and cannot be accepted!", true));
					result = false;
				}
				else
				{
					result = (instance.ShellCount < instance.MaximumShellCount * clip.scaleFactor && instance.ShellCount < instance.MaximumRoundCountForDiameter(diameter) * clip.scaleFactor);
				}
				return result;
			}
			else
			{
				double num = 1E-05;
				bool flag = (double)length > (double)instance.LengthCapacity + num;
				bool result;
				if (flag)
				{
					InfoStore.Add(ShellRackPhysicalContainer._locFile.Get("Error_ShellTooLong", "Shell is too long for the shell rack- and cannot be accepted!", true));
					result = false;
				}
				else
				{
					bool flag2 = instance.ShellCount >= instance.MaximumShellCount;
					result = (!flag2 && instance.ShellCount < instance.MaximumRoundCountForDiameter(diameter));
				}
				return result;
			}
		}

		public static int MaximumRoundCountForDiameter(ShellRackPhysicalContainer instance, int diameter)
		{
			bool flag = diameter == 0;
			int result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				int num = 1000 / diameter;
				bool flag2 = diameter <= 250;
				if (flag2)
				{
					num *= 2;
				}
				result = Math.Min(instance.MaximumShellCount, num);
			}
			if (instance is CompressedShellRackPhysicalContainer cInstance)
			{
				CompressedAdvCannonAmmoClip clip = cInstance.AmmoClip as CompressedAdvCannonAmmoClip;
				if (clip == null) throw new Exception("A compressed shell rack container was added to an uncompressed clip!");
				return (int)(result * clip.scaleFactor);
			}
			return result;
		}
	}
	public class CompressedAdvCannonAmmoClip : AdvCannonAmmoClip
	{
		public int CompressionLevel;
		public float scaleFactor;
		static List<Guid> overriddenGuids = new List<Guid>();

		public void TryItemOverride()
		{
			CompressionLevel = item.Code.Variables.GetInt("CompressionLevel");
			scaleFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionScale"];
			float costFactor = (float)Plugin.cfg["Level" + CompressionLevel.ToString() + "CompressionCost"];
			if (!overriddenGuids.Contains(item.ComponentId.Guid))
			{
				this.item.Weight *= scaleFactor;
				this.item.Health *= scaleFactor;
				this.item.Cost.Material *= costFactor;

				overriddenGuids.Add(item.ComponentId.Guid);
			}
		}

		public override void ItemSet()
		{
			TryItemOverride();
			base.ItemSet();
		}

		public override void ComponentStart()
		{
			this.Data.Clip = this;
			this.ShellContainer = new CompressedShellRackPhysicalContainer(this)
			{
				LengthCapacity = this.LengthCapacity,
				MaximumShellCount = 64,
				RenderShells = this.RenderShells,
				ShellMaterialComponentRef = base.item.MaterialReference
			};
			bool isInGame = Get.IsInGame;
			if (isInGame)
			{
				this.SpawnVisualShellHolder();
			}
		}
	}
}