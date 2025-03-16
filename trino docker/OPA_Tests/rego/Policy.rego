package policy

import rego.v1

default allow := false

#allow all highest level access
allow if {
	input.action.operation == "ExecuteQuery"
}

#allow catalog - only iceberg
allow if {
	input.action.operation == "AccessCatalog"
	input.action.resource.catalog.name == "iceberg"
}

allow if {
	input.action.operation == "SelectFromColumns"
	user_in_correct_group
}

default user_roles := false

groups_for_object contains group if {
	some i
	data.Perms[i].Project == input.action.resource.table.schemaName
	group := data.Perms[i].Group
}

user_in_correct_group if {
	some i, j
	data.GroupMembers[i].Username == input.context.identity.user
	data.GroupMembers[i].Group = groups_for_object[j]
}
