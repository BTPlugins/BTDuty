# BTDuty
Your normal Duty Plugin with more Features!
<br/>
## Features
- Easy to Configure. Unlike the old Duty Plugin
- On / Off Duty Webhooks
- TimeClock
- Sends a Webhook every X amount of Seconds of all Active People on Duty
<br/>

## REQUIREMENTS
- ShimmyMySherbet.DiscordWebhooks.Embeded

## Commands
- **/Duty** &lt;Duty Tag> | ``BTDuty.Duty``
- **/ListTags** |  ``BTDuty.ListTags``
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
  <DutyLogWebhook>https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}</DutyLogWebhook>
  <DutyGroups>
    <Group>
      <DutyName>Helper</DutyName>
      <GroupID>Helper</GroupID>
      <Permission>BTDuty.Helper</Permission>
    </Group>
    <Group>
      <DutyName>Mod</DutyName>
      <GroupID>Moderator</GroupID>
      <Permission>BTDuty.Moderator</Permission>
    </Group>
    <Group>
      <DutyName>Admin</DutyName>
      <GroupID>Administrator</GroupID>
      <Permission>BTDuty.Administrator</Permission>
    </Group>
  </DutyGroups>
  <ActiveDutyList>
    <Enabled>true</Enabled>
    <Timer>300</Timer>
    <WebhookURL>https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}</WebhookURL>
  </ActiveDutyList>
  <DebugMode>false</DebugMode>
</BTDutyConfiguration>
```

## Get the Plugin Here
- https://github.com/BTPlugins/BTDuty/releases/tag/Release

## Discord Support: 
- https://discord.gg/YsaXwBSTSm
