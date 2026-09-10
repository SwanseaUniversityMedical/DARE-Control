# DARE-Control

To connect them together
u = sailegressapi
pass = password123

## Local development against a kind cluster

`src/Data-Egress-API` and `src/Data-Egress-UI` each carry an
`appsettings.Development_Kind.json` profile for running natively from the host (VS Code /
`dotnet run`) against a local kind cluster brought up by the `5s-Tes` repo's
`dev-env-setup/cluster-setup.sh`, with the egress product enabled per that repo's
`charts/agent-devstack/README.md` **Optional: local Data Egress** section. The profile points at
the agent family's `dev-access` NodePorts (`charts/agent-devstack/templates/dev-access.yaml`) and
the local Keycloak ingress (`keycloak.agent.localtest.me`), and uses the agent-devstack chart's
fixed dev Secrets (`devsecret-egress-api`, `devsecret-egress-ui`) and dev realm user
(`dev`/`password123`, realm `Data-Egress`) — the documented dev/prod interface, not new secret
material.

Select the profile with `ASPNETCORE_ENVIRONMENT=Development_Kind`; give each app its own
`ASPNETCORE_URLS`:

| App | Run from host | Talks to |
|---|---|---|
| `Data-Egress-API` | `ASPNETCORE_ENVIRONMENT=Development_Kind ASPNETCORE_URLS=http://localhost:5084 dotnet run --no-launch-profile` | egress Postgres/RustFS/Seq (agent `dev-access` NodePorts), Keycloak `http://keycloak.agent.localtest.me/realms/Data-Egress` and `.../realms/Dare-TRE`, the agent product's api at `http://agent-api.agent.localtest.me` |
| `Data-Egress-UI` | `ASPNETCORE_ENVIRONMENT=Development_Kind ASPNETCORE_URLS=http://localhost:5085 dotnet run --no-launch-profile` | the `Data-Egress-API` above (`http://localhost:5084`), the same Keycloak realm |

`--no-launch-profile` is required: `Properties/launchSettings.json`'s own `ASPNETCORE_ENVIRONMENT=Development`
otherwise wins over a shell-exported value. Confirmed live: `Hosting environment: Development_Kind`
in the boot log, `appsettings.Development_Kind.json` picked up, `GET /health` returns `Healthy`.

**Avoiding double consumers**: running an app from the host while its in-cluster copy is also
running means two processes sharing one Postgres row set. Before running `Data-Egress-API` from
the host, scale down its in-cluster copy (keep supplying the egress product's own `-f` values
file so nothing else in it resets):

```bash
helm upgrade egress /path/to/DARE-Control/charts/egress \
  --namespace 5s-tes-agent -f <the egress product values file> \
  --set api.enabled=false --kube-context kind-5s-tes

# restore afterwards
helm upgrade egress /path/to/DARE-Control/charts/egress \
  --namespace 5s-tes-agent -f <the egress product values file> --kube-context kind-5s-tes
```
