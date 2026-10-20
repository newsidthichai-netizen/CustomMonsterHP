using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace CustomMonsterHP
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            // ดักอีเวนต์เมื่อตัวละคร/มอนสเตอร์ถูกเพิ่มเข้ามาในฉาก
            helper.Events.World.NpcListChanged += this.OnNpcListChanged;
        }

        private void OnNpcListChanged(object sender, NpcListChangedEventArgs e)
        {
            // ตรวจสอบมอนสเตอร์ที่เกิดใหม่
            foreach (var npc in e.Added)
            {
                if (npc is Monster monster)
                {
                    // กำหนดค่าเลือดตรงๆ เป็นจำนวนเต็ม เช่น ตั้งให้เลือดเป็น 500 ทุกตัว
                    // หรือจะเขียนเงื่อนไขแยกตามชื่อมอนสเตอร์ก็ได้ครับ
                    monster.setMaxHealth(500);
                    monster.health = 500;

                    this.Monitor.Log($"[CustomMonsterHP] ปรับเลือดของ {monster.Name} เป็น 500 เรียบร้อย!", LogLevel.Debug);
                }
            }
        }
    }
}
