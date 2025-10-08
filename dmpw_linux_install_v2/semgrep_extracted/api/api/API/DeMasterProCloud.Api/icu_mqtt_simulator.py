import json
from multiprocessing.pool import ThreadPool
from random import random, randint

import requests
import paho.mqtt.client as mqtt
import time
from datetime import datetime
import uuid
import sys

ONLINE_TOPIC = '/topic/online'
EVENT_LOG_BASE_TOPIC = '/topic/event'
MESSAGE = {
    "msgId": "ICU_ADDRESS_0f8fad5b-d9cb-469f-a165-70867728950e",
    "type": "EVENT_LOG",
    "data": {
        "total": 1,
        "events": [
        ]
    }
}

EXCLUDE_ICU_ADDRESS = ['450050', '840000', '01E208', '01E256', '024A01']

started = 0

if __name__ == '__main__':

    number_of_thread = int(sys.argv[1])
    number_of_event_sending = int(sys.argv[2])
    time_delay = float(sys.argv[3])
    api_url = sys.argv[4]
    broker_url = sys.argv[5]
    payload = {
        "username": "admin@gmail.com",
        "password": "123456789",
    }
    req = requests.post(api_url + "/login", json=payload,
                        headers={"content-type": "application/json"})

    token = req.json()['authToken']
    # Get list of device
    req2 = requests.get(api_url + "/devices?pageSize=1000",
                        headers={"content-type": "application/json", "Authorization": "Bearer %s" % token})
    json_response_2 = req2.json()

    icu_addresses = [device['deviceAddress'] for device in json_response_2['data'] if
                     (device['deviceAddress'] not in EXCLUDE_ICU_ADDRESS)]

    # get list of user
    req3 = requests.get(api_url + "/users?pageSize=1000",
                        headers={"content-type": "application/json", "Authorization": "Bearer %s" % token})
    json_response_3 = req3.json()
    user_cardids = []
    for user in json_response_3['data']:
        for card in user['cardList']:
            if card['cardId'] or card['cardId'] != "string":
                user_cardids.append(card['cardId'])


    def connect_and_publish(i):
        icu_address = icu_addresses[i]

        topic = EVENT_LOG_BASE_TOPIC + '/%s' % icu_address
        client = mqtt.Client(
            client_id="ICU_%s" % icu_address,
            clean_session=False,
            protocol=mqtt.MQTTv311,
        )
        client.username_pw_set('icuclient', password='icuclient123')
        client.will_set(ONLINE_TOPIC, '{"msgId":"0f8fad5b-d9cb-469f-a165-70867728950e","type":"CONNECTION_STATUS","data":{"deviceAddress":"%s","ipAddress":"192.168.1.1","status":0,"deviceType": "ICU-300","macAddress": "00:3F:33:00:00:20"}}' % icu_address, qos=1, retain=False)
        client.connect(host=broker_url, keepalive=1, port=1883)
        send_nums = number_of_event_sending
        while (send_nums > 0):
            message = {
                "type": "EVENT_LOG",
                "data": {
                    "total": 1,
                    "events": [
                    ]
                }
            }
            now = datetime.now()
            events = [{
                "deviceAddress": icu_address,
                "accessTime": now.strftime("%d%m%Y%H%M%S"),
                "cardId": user_cardids[randint(0, len(user_cardids) - 1)],
                "issueCount": "1",
                "username": None,
                "updateTime": now.strftime("%d%m%Y%H%M%S%f"),
                "inOut": "In",
                "eventType": (send_nums % 3 + 1)
            }]
            message['data']['events'] = events
            message['msgId'] = "%s_%s" % (icu_address, uuid.uuid4())
            global started
            started = started + 1
            print ("%s: ICU %s publishing... %s to topic %s" % (started, icu_address, send_nums, topic))

            client.publish(topic, json.dumps(message), retain=False)
            print (json.dumps(message))

            send_nums -= 1
            time.sleep(time_delay)
           
    print (number_of_thread)
    pool = ThreadPool(number_of_thread)
    results = pool.map(connect_and_publish, range(number_of_thread))
    #results = pool.map(create_user, range(number_of_thread))

    # close the pool and wait for the work to finish
    pool.close()
    pool.join()