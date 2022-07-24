using Rocket.API.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTDuty
{
    public partial class DutyPlugin
    {
        public override TranslationList DefaultTranslations => new TranslationList
        {
            {
                "ProperUsage", "[color=#FF0000]{{BTDuty}} [/color] [color=#F3F3F3]Proper Usage |[/color] [color=#3E65FF]{0}[/color]"
            },
            {
                "TargetNotFound", "[color=#FF0000]{{BTDuty}} [/color][color=#F3F3F3]Target Not Found![/color]"
            },
            {
                "ErrorContactStaff", "[color=#FF0000]{{BTDuty}} [/color][color=#3E65FF]Error Found[/color][color=#F3F3F3]! Please contact Staff about the issue![/color]"
            },
            {
                "Duty_MissingPerm", "[color=#FF0000]{{BTDuty}} [/color][color=#F3F3F3]Missing Permission:[/color] [color=#3E65FF]{0}[/color] [color=#F3F3F3]to go OnDuty as [/color][color=#3E65FF]{1}[/color]"
            },
            {
                "ListTags_List", "[color=#FF0000]{{BTDuty}} [/color][color=#F3F3F3]Available Tags:[/color] [color=#3E65FF]{0}[/color]"
            },
            {
                "Duty_OffDuty", "[color=#FF0000]{{BTDuty}} [/color][color=#F3F3F3]You have Successfully Clocked Off Duty as a [/color] [color=#3E65FF]{0}[/color][color=#F3F3F3]! Total Time: [/color][color=#3E65FF]{1}[/color]"
            },
            {
                "Duty_OnDuty", "[color=#FF0000]{{BTDuty}} [/color][color=#F3F3F3]You have successfully Clocked on as[/color] [color=#3E65FF]{0}[/color]"
            },
            {
                "Broadcast_OffDuty", "[color=#FF0000]{{BTDuty}} [/color][color=#3E65FF]{0}[/color] [color=#F3F3F3]has Clocked off Duty for[/color][color=#3E65FF] {1}[/color]"
            },
            {
                "Broadcast_OnDuty", "[color=#FF0000]{{BTDuty}} [/color][color=#3E65FF]{0}[/color] [color=#F3F3F3]has Clocked On Duty for[/color][color=#3E65FF] {1}[/color]"
            },
            {
                "TimeClock_NotOnDuty", "[color=#FF0000]{{BTDuty}} [/color][color=#3E65FF]{0}[color=#F3F3F3] is currently not on Duty![/color]"
            },
            {
                "TimeClock_OnDuty", "[color=#FF0000]{{BTDuty}} [/color][color=#3E65FF]{0}[/color][color=#F3F3F3] TimeClock | Duty Name:[/color] [color=#3E65FF]{1}[/color] [color=#F3F3F3]| TimeClock:[/color][color=#3E65FF] {2}[/color]"
            },
            {
                "DutyGroup_NotFound", "[color=#FF0000]{{BTDuty}} [/color][color=#3E65FF]{0}[/color][color=#F3F3F3] Duty Group not found![/color]"
            },
            {
                "DamageCanceled", "[color=#FF0000]{{BTDuty}} [/color][color=#F3F3F3]Unable to Damage [/color][color=#3E65FF]{0} [/color][color=#F3F3F3]while being on Duty![/color]"
            }
        };
    }
}