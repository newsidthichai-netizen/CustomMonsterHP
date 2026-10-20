using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace CustomMonsterHP
{
    // 1. กำหนดรูปแบบไฟล์ Config สำหรับเซฟค่า
    public class ModConfig
    {
        public float HpMultiplier { get; set; } = 3.0f; // ค่าเริ่มต้นคือเพิ่มเลือด 300% (คูณ 4)
    }

    // 2. อินเทอร์เฟซสำหรับเชื่อมต่อกับม็อด GMCM
    public interface IGenericModConfigMenuApi
    {
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);
        void AddNumberOption(IManifest mod, Func<float> getValue, Action<float> setValue, Func<string> name, Func<string> tooltip = null, float? min = null, float? max = null, float? interval = null, Func<float, string> formatValue = null, string fieldId = null);
    }

    // 3. โค้ดการทำงานหลักของม็อด
    public class ModEntry : Mod
    {
        private ModConfig Config;

        public override void Entry(IModHelper helper)
        {
            // อ่านค่าจาก config.json
            this.Config = this.Helper.ReadConfig<ModConfig>();

            // ดักจับเมื่อมอนสเตอร์เกิดในแมพ
            helper.Events.World.NpcListChanged += OnNpcListChanged;

            // ดักจับเมื่อเกมเปิดขึ้นมา เพื่อเชื่อมระบบกับ GMCM
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // ดึงระบบ GMCM มาใช้งาน
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // เพิ่มชื่อม็อดเราลงในหน้าเมนู GMCM
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );

            // สร้างปุ่มเลื่อนปรับตัวเลขคูณเลือดมอนสเตอร์ในเมนูเกม
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                getValue: () => this.Config.HpMultiplier,
                setValue: value => this.Config.HpMultiplier = value,
                name: () => "Monster HP Multiplier",
                tooltip: () => "ปรับเพิ่มเลือดมอนสเตอร์ (เช่น 1.0 = ปกติ, 3.0 = เพิ่มเลือด 300%)",
                min: 0.0f,
                max: 10.0f,
                interval: 0.1f
            );
        }

        private void OnNpcListChanged(object sender, NpcListChangedEventArgs e)
        {
            foreach (NPC npc in e.Added)
            {
                if (npc is Monster monster)
                {
                    // เช็กว่ามอนสเตอร์ตัวนี้เคยปรับเลือดไปหรือยัง (ป้องกันคูณซ้ำ)
                    if (!monster.modData.ContainsKey("CustomMonsterHP_Scaled"))
                    {
                        int originalHp = monster.MaxHealth;
                        int newHp = (int)(originalHp * (1f + this.Config.HpMultiplier));

                        monster.MaxHealth = newHp;
                        monster.Health = newHp;

                        // แปะป้ายไว้ว่าปรับเลือดเรียบร้อยแล้ว
                        monster.modData["CustomMonsterHP_Scaled"] = "true";
                    }
                }
            }
        }
    }
}
