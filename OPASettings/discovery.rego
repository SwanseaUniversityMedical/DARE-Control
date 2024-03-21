package play
import rego.v1
import input
default allow = false

allow if {
   print("input", input)
   input.identity.user == input.resource.table.schemaName
}