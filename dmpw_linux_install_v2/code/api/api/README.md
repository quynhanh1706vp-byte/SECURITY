# Project DeMasterProCloud

DeMasterPro Cloud solution for  access control system

[VERSION: 2.2.x]

### [NOTE] API special

- PUT /roles/default: update default permissions (SystemAdmin)
- PATCH /mail-templates/update-default: create mail template default if null. Using for case develop create new mail template (SystemAdmin)
- GET /cameras/check/sync-user: check sync data of users in system and in hanet system (PrimaryManager)
- POST /cameras/fix/sync-user: delete all user and card HFaceId error in system or hanet (PrimaryManager)
- PATCH /system-info/rabbitmq/reset-permission: update permission to account rabbitmq (SystemAdmin)
- PATCH /system-info/user-avatar/reset-link-domain: update migration avatar user Ex http://xxx.xxx.xxx.xxx => https://yyy.yyy.yyy.yyy (SystemAdmin)
- PATCH /system-info/event-log/reset-link-image: update migration link image hanet EventLog Ex http://xxx.xxx.xxx.xxx => https://yyy.yyy.yyy.yyy (SystemAdmin)

### [Cronjob]

- Schedule every day:
  - 0h00: Update time device
  - 0h30: Check and send mail notify over due book
  - 22h00: Auto delete notification (current limit stored in database is 30 days)
  - 23h00: Backup file (videos, images) to S3
  - 23h30: Check company photos and videos storage limits
- Schedule every time:
  - 60 seconds/time: Recheck attendance (all companies not use calculate real-time)
  - 60 seconds/time: Get image from camera hanet to add event-log
  - 120 seconds/time: Send device instruction set time to all devices
  - 120 seconds/time: Send get device info set time to all devices
  - 30 minutes/time: Check visitor expired
  - 2 minutes/time: check meeting setting to device
  - 4 hours/time: create new attendance record to all users
- Others:
  - 0h00 on 1st every month: backup event log, system log