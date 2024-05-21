package policy

data = {
    "Perms": [{
            "Project": "sail0675v",
            "Group": "SAIL_0675_Developer",
            "PermType": "rw",
            "Object": ""
        }
    ],
    "GroupMembers": [{
            "Group": "SAIL_0675_Developer",
            "Username": "rawlinga"
        }
    ]
}


intput = {
        "context": {
            "identity": {
                "user": "rawlinga",
                "groups": []
            },
            "softwareStack": {
                "trinoVersion": "444"
            }
        },
        "action": {
            "operation": "SelectFromColumns",
            "resource": {
                "table": {
                    "catalogName": "postgresql",
                    "schemaName": "sail0675v",
                    "tableName": "cool",
                    "columns": ["aaaaaa", "id"]
                }
            }
        }
    }

test_WHY_NO_WORK {
  allow 
  with input as intput
  with data as data
}
