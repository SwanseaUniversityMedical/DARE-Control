package play

import rego.v1
import input

default allow = false

allow if {
   print("input", input)
   input.context.identity.user == input.action.resource.table.schemaName
}