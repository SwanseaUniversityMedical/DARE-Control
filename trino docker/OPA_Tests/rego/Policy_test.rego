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

test_Default {
  allow 
  with input as intput
  with data as data
}


intput2 = {
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
                    "schemaName": "nottheOne",
                    "tableName": "cool",
                    "columns": ["aaaaaa", "id"]
                }
            }
        }
    }

test_schemaName_diff {
  not allow 
  with input as intput2
  with data as data
}



intpu3 = {
        "context": {
            "identity": {
                "user": "bob",
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

test_user_not_in {
  not allow 
  with input as intpu3
  with data as data
}
