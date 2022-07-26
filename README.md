# BTDuty
Your normal Duty Plugin with more Features!
<br/>
## Features
- Easy to Configure. Unlike the old Duty Plugin
- On / Off Duty Webhooks
- TimeClock
- Sends a Webhook every X amount of Seconds of all Active People on Duty
- Logs such as Command Usage and Item Pickups while on Duty!
<br/>

## REQUIREMENTS
- ShimmyMySherbet.DiscordWebhooks.Embeded

## Commands
- **/Duty** &lt;Duty Tag> | ``BTDuty.Duty``
- **/DutyTags** |  ``BTDuty.ListTags``
- **/TimeClock** | ``BTDuty.TimeClock``
<br />

## Configuration
```
<?xml version="1.0" encoding="utf-8"?>
<BTDutyConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <ServerAnnouncer>
    <Enabled>false</Enabled>
    <BypassPermission>NoSendMessage.IfHaveThisPerm</BypassPermission>
  </ServerAnnouncer>
  <RemoveDutyOnLogout>false</RemoveDutyOnLogout>
  <RemoveBlueHammerOnLogout>false</RemoveBlueHammerOnLogout>
  <AllowDamageToPlayers>true</AllowDamageToPlayers>
  <WebhookContainer>
    <DutyLogWebhook>https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}</DutyLogWebhook>
    <ActiveDutyWebhook>https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}</ActiveDutyWebhook>
    <CommandWebhook>https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}</CommandWebhook>
    <ItemAddedWebhook>https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}</ItemAddedWebhook>
  </WebhookContainer>
  <DutyGroups>
    <Group>
      <DutyName>Helper</DutyName>
      <GroupID>Helper</GroupID>
      <Permission>BTDuty.Helper</Permission>
      <BlueHammer>false</BlueHammer>
      <Godmode>true</Godmode>
      <Vanish>true</Vanish>
    </Group>
    <Group>
      <DutyName>Mod</DutyName>
      <GroupID>Moderator</GroupID>
      <Permission>BTDuty.Moderator</Permission>
      <BlueHammer>false</BlueHammer>
      <Godmode>true</Godmode>
      <Vanish>true</Vanish>
    </Group>
    <Group>
      <DutyName>Admin</DutyName>
      <GroupID>Administrator</GroupID>
      <Permission>BTDuty.Administrator</Permission>
      <BlueHammer>true</BlueHammer>
      <Godmode>false</Godmode>
      <Vanish>false</Vanish>
    </Group>
  </DutyGroups>
  <ActiveDutyList>
    <Enabled>true</Enabled>
    <Timer>300</Timer>
  </ActiveDutyList>
  <RestrictionsHolder>
    <BypassPermission>Ignore.CommandRestrictions</BypassPermission>
    <RestrictedCommand>
      <CommandName>Admin</CommandName>
      <CommandName>Slay</CommandName>
      <CommandName>frisk</CommandName>
    </RestrictedCommand>
  </RestrictionsHolder>
  <DebugMode>false</DebugMode>
</BTDutyConfiguration>
```

## Get the Plugin Here
- https://github.com/BTPlugins/BTDuty/releases/tag/Release

## Discord Support: 
- https://discord.gg/YsaXwBSTSm
