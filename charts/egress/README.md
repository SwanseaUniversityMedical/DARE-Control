# egress

Standalone chart for the Data-Egress product.

## What this chart deploys

- **api** — the Data-Egress API (`control-egress-api` image), listening on `/health` at port 8080.
- **ui** — the Data-Egress web UI (`control-egress-ui` image), listening on `/health` at port 8080.

`global.tag` must be a `control-egress-api`/`control-egress-ui` release built at or after the
commit that adds `/health`. **This must be `3.0.5` or later or it won't work**.

Both components are fixed at `replicas: 1` (the UI keeps sessions in memory). Neither needs a
persistent volume.

## What must already exist

- The two Secrets listed below.
- A reachable `Data-Egress` Keycloak realm at `global.oidc.authority`, and a reachable `Dare-TRE`
  Keycloak realm at `tre.oidcAuthority` (the API's own outbound service-account credential for
  calling the TRE API).
- A reachable PostgreSQL database, RustFS (S3-compatible) endpoint, TRE API and Seq instance, at
  the addresses given by the `connectionString` secret, `api.s3Url`, `api.treApiAddress` and
  `global.config.seqUrl`.
- The cluster CA bundle ConfigMap named by `global.trustClusterCa.configMapName`, if
  `global.trustClusterCa.enabled` is `true`.

## What this chart does not do

It does not install PostgreSQL, RustFS, Seq or Keycloak. It does not create any Secret. It
contains no ArgoCD `Application` and no `dependencies:`. The `5s-Tes` repo's `charts/agent-stack`
chart installs all of the above.

## Encryption key stability

`EncryptionSettings__Key`/`__Base` (in `egress-api-secret`) decrypt Keycloak admin credentials
already written to the `DATA-Egress` Postgres database. Rotating either value without a
re-encryption migration makes every existing stored credential permanently undecryptable.

## Secrets

This chart does **not** create these. They must already exist in the namespace before the chart
is installed. The `5s-Tes` repo's `charts/agent-stack` chart creates them from Vault.

### `egress-api-secret`

Set by `api.secretName`.

| **Key** | **Used for** | **Required** |
|---|---|---|
| `connectionString` | PostgreSQL connection string, including the password. Read into `ConnectionStrings__DefaultConnection`. | Yes |
| `treKeycloakClientSecret` | Client secret for the `Dare-TRE-API` Keycloak client. Read into `TreKeyCloakSettings__ClientSecret`. | Yes |
| `dataEgressKeycloakClientSecret` | Client secret for the API's own `Data-Egress-API` Keycloak client. Read into `DataEgressKeyCloakSettings__ClientSecret`. | Yes |
| `s3AccessKey` | Access key for the RustFS bucket. Read into `MinioSettings__AccessKey`. | Yes |
| `s3SecretKey` | Secret key matching `s3AccessKey`. Read into `MinioSettings__SecretKey`. | Yes |
| `encryptionKey` | AES-128 key (16-byte Base64) that decrypts DB-stored Keycloak admin credentials. Read into `EncryptionSettings__Key`. See **Encryption key stability** above. | Yes |
| `encryptionBase` | AES IV matching `encryptionKey` (named "Base" in the app's own config). Read into `EncryptionSettings__Base`. | Yes |
| `demoModeDefaultPassword` | Seeded Keycloak password for the two demo service-account credential rows, written to the database when `api.demoMode` is `"true"`. Read into `DemoModeDefaultP`. | Yes |

### `egress-ui-secret`

Set by `ui.secretName`.

| **Key** | **Used for** | **Required** |
|---|---|---|
| `keycloakClientSecret` | Client secret for the `Data-Egress-UI` Keycloak client. Read into `DataEgressKeyCloakSettings__ClientSecret`. | Yes |

## Parameters

### Common parameters

| **Name** | **Description** | **Value** |
|---|---|---|
| `nameOverride` | Replaces the chart name in object names. | `""` |
| `fullnameOverride` | Replaces the full prefix on every object name. | `"egress"` |
| `imagePullSecrets` | Secrets used to pull images from a private registry. | `[]` |
| `serviceAccount.create` | Create a service account for the pods. | `true` |
| `serviceAccount.annotations` | Annotations on the ServiceAccount. | `{}` |
| `serviceAccount.name` | Overrides the ServiceAccount name. Empty uses the chart's fullname. | `""` |
| `podSecurityContext` | Pod-level securityContext. `fsGroup`/`fsGroupChangePolicy` let UID 1000 (`securityContext.runAsUser`) write the `/tmp` emptyDir consistently. | `{fsGroup: 1000, fsGroupChangePolicy: OnRootMismatch}` |
| `securityContext.runAsUser` | User ID both containers run as. | `1000` |
| `securityContext.runAsGroup` | Group ID both containers run as. | `1000` |
| `securityContext.runAsNonRoot` | Stop containers running as root. Do not change without a reason in the pull request. | `true` |
| `securityContext.readOnlyRootFilesystem` | Make the container filesystem read only. | `true` |
| `securityContext.allowPrivilegeEscalation` | Stop a process gaining more privileges than the one that started it. | `false` |
| `securityContext.capabilities.drop` | Linux capabilities dropped from both containers. | `["ALL"]` |

### DataProtection

| **Name** | **Description** | **Value** |
|---|---|---|
| `dataProtection.persistKeys` | Persist ASP.NET data-protection keys to disk. Leave `"false"`: the chart mounts no key volume. | `"false"` |
| `dataProtection.keysPath` | Value of `DataProtectionSettings__KeysPath`. This chart mounts no volume at this path: turning `persistKeys` on without adding one first fails the pod at startup under `readOnlyRootFilesystem: true`. | `/keys` |

### Global parameters

Settings shared by more than one component. Defined once.

| **Name** | **Description** | **Value** |
|---|---|---|
| `global.tag` | Image tag used by a component that does not pin its own. `control-egress-api` and `control-egress-ui` are built and released together, so one tag covers both. | `"3.0.4"` |
| `global.config.aspnetEnvironment` | Value of `ASPNETCORE_ENVIRONMENT` in both components. | `"Development"` |
| `global.config.seqUrl` | Address of the Seq instance both components log to. Read into `Serilog__SeqServerUrl`. | `"http://seq:5341"` |
| `global.config.allowedHosts` | ASP.NET Core host-filtering value, identical in both components. Read into `AllowedHosts`. | `"*"` |
| `global.config.logging.default` | Read into `Logging__LogLevel__Default` in both components (framework-internal logs only; not this app's own log statements). | `"Information"` |
| `global.config.logging.microsoftAspNetCore` | Read into `Logging__LogLevel__Microsoft.AspNetCore` in both components. | `"Warning"` |
| `global.config.serilog.default` | Read into `Serilog__MinimumLevel__Default` in both components — the effective log-verbosity knob. | `"Verbose"` |
| `global.config.serilog.overrideMicrosoft` | Read into `Serilog__MinimumLevel__Override__Microsoft`. | `"Warning"` |
| `global.config.serilog.overrideEfCoreModelValidation` | Read into `Serilog__MinimumLevel__Override__Microsoft.EntityFrameworkCore.Model.Validation`. | `"Error"` |
| `global.config.serilog.overrideSystem` | Read into `Serilog__MinimumLevel__Override__System`. | `"Warning"` |
| `global.config.serilog.overrideHangfire` | Read into `Serilog__MinimumLevel__Override__Hangfire`. | `"Warning"` |
| `global.oidc.authority` | Full `Data-Egress` Keycloak realm URL both components authenticate against. Bare realm URL: no trailing slash, no `.well-known` suffix. | `"http://keycloak/realms/Data-Egress"` |
| `global.monitoring.enabled` | Push metrics to a Prometheus Pushgateway. | `false` |
| `global.monitoring.pushgatewayUrl` | Pushgateway address, used when `global.monitoring.enabled` is `true`. | `""` |
| `global.ingress.enabled` | Create an Ingress for either component at all. | `true` |
| `global.ingress.className` | Ingress controller class for every Ingress. | `"nginx"` |
| `global.ingress.host` | Base domain. `api.ingress.host`/`ui.ingress.host` default to a subdomain of this when left empty. | `"localtest.me"` |
| `global.ingress.certClusterIssuer` | cert-manager ClusterIssuer that issues each Ingress's TLS certificate. | `"ca-issuer"` |
| `global.ingress.tls` | Terminate TLS at the ingress. Each Ingress declares its own certificate. | `true` |
| `global.trustClusterCa.enabled` | Mount a cluster CA bundle over both containers' trust store. Both components call Keycloak over HTTPS. | `false` |
| `global.trustClusterCa.configMapName` | ConfigMap holding the bundle. Provided by the cluster, not by this chart. | `overlay-castore` |
| `global.trustClusterCa.key` | Key inside that ConfigMap. Also used as the mount `subPath`. | `ca-certificates.crt` |
| `global.trustClusterCa.mountPath` | File replaced inside the container. Correct for Debian, Ubuntu and Alpine images. | `/etc/ssl/certs/ca-certificates.crt` |
| `tre.oidcAuthority` | Full `Dare-TRE` Keycloak realm URL. The API obtains a service token from this realm to call the TRE API; distinct from `global.oidc.authority`. | `"http://keycloak/realms/Dare-TRE"` |

### API parameters

| **Name** | **Description** | **Value** |
|---|---|---|
| `api.enabled` | Deploy the API component. | `true` |
| `api.image.repository` | Image for the API. | `harbor.ukserp.ac.uk/dare-trefx/control-egress-api` |
| `api.image.tag` | Image tag. Falls back to `global.tag` when empty. | `""` |
| `api.image.pullPolicy` | Image pull policy for the API. | `IfNotPresent` |
| `api.containerPort` | Port the ASP.NET app listens on inside the container. | `8080` |
| `api.resources` | Container resource requests/limits. | `{}` |
| `api.service.type` | API Service type. | `ClusterIP` |
| `api.secretName` | Name of the Kubernetes Secret holding this component's secrets. See **Secrets** above. | `egress-api-secret` |
| `api.ingress.enabled` | Create an Ingress for the API. | `true` |
| `api.ingress.host` | Hostname for the API Ingress. Empty computes `egress-api.<global.ingress.host>`. | `""` |
| `api.demoMode` | Seed demo data on startup. Always rendered: an unset `DemoMode` crashes the app at startup. | `"false"` |
| `api.keycloakDemoMode` | Run the API's Keycloak integration in demo mode. Always rendered: an unset `KeycloakDemoMode` crashes the app at startup. | `"false"` |
| `api.suppressAntiforgery` | Disable antiforgery checks. | `"false"` |
| `api.oidc.clientId` | Keycloak client ID for the API's own `Data-Egress-API` client. | `Data-Egress-API` |
| `api.oidc.validAudiences` | Accepted token audiences. Always rendered: an unset value crashes the app at startup. | `Data-Egress-UI,Data-Egress-API` |
| `api.oidc.proxy` | The API sits behind an egress proxy when calling Keycloak. | `"false"` |
| `api.oidc.proxyAddresUrl` | Proxy address, used when `api.oidc.proxy` is `"true"`. | `""` |
| `api.oidc.bypassProxy` | Comma-separated proxy bypass list. | `""` |
| `api.treOidc.clientId` | Keycloak client ID for the API's outbound `Dare-TRE-API` service account. | `Dare-TRE-API` |
| `api.treOidc.proxy` | The API sits behind an egress proxy when calling the `Dare-TRE` realm. | `"false"` |
| `api.treOidc.proxyAddresUrl` | Proxy address, used when `api.treOidc.proxy` is `"true"`. | `""` |
| `api.s3Url` | In-cluster RustFS S3 endpoint. Read into `MinioSettings__Url`. | `http://rustfs-svc:9000` |
| `api.s3Proxy.enabled` | Route RustFS S3 calls through a proxy. | `"false"` |
| `api.s3Proxy.addressUrl` | Proxy address, used when `api.s3Proxy.enabled` is `"true"`. | `""` |
| `api.s3Proxy.bypassProxy` | Proxy bypass list for RustFS S3 calls. | `""` |
| `api.treApiAddress` | Address of the TRE API. Static Service name of the agent chart's API component. | `http://agent-api` |
| `api.email.host` | SMTP host for admin-notification email. | `192.168.10.22` |
| `api.email.port` | SMTP port. | `25` |
| `api.email.enableSsl` | Use SSL for the SMTP connection. | `"false"` |
| `api.email.fromAddress` | From address for admin-notification email. | `helpdesk@chi.swan.ac.uk` |
| `api.email.fromDisplayName` | From display name for admin-notification email. | `SERP Gov` |
| `api.email.override` | When set, replaces every notification recipient with this single address. | `""` |
| `api.email.enabled` | Enable outbound email. | `"false"` |
| `api.extraEnv` | Extra environment variables. | `[]` |

### UI parameters

| **Name** | **Description** | **Value** |
|---|---|---|
| `ui.enabled` | Deploy the UI component. | `true` |
| `ui.image.repository` | Image for the UI. | `harbor.ukserp.ac.uk/dare-trefx/control-egress-ui` |
| `ui.image.tag` | Image tag. Falls back to `global.tag` when empty. | `""` |
| `ui.image.pullPolicy` | Image pull policy for the UI. | `IfNotPresent` |
| `ui.containerPort` | Port the ASP.NET app listens on inside the container. | `8080` |
| `ui.resources` | Container resource requests/limits. | `{}` |
| `ui.service.type` | UI Service type. | `ClusterIP` |
| `ui.service.annotations` | Annotations for the UI Service, e.g. a static LoadBalancer IP when exposed externally. | `{}` |
| `ui.secretName` | Name of the Kubernetes Secret holding this component's secrets. See **Secrets** above. | `egress-ui-secret` |
| `ui.ingress.enabled` | Create an Ingress for the UI. | `true` |
| `ui.ingress.host` | Hostname for the UI Ingress. Empty computes `egress.<global.ingress.host>`. | `""` |
| `ui.demoMode` | No effect in the UI. Always rendered: an unset `DemoMode` crashes the app at startup. | `"false"` |
| `ui.keycloakDemoMode` | No effect in the UI. Always rendered: an unset `KeycloakDemoMode` crashes the app at startup. | `"false"` |
| `ui.suppressAntiforgery` | Disable antiforgery checks. | `"false"` |
| `ui.sslCookies` | Mark cookies secure. Requires HTTPS end-to-end if `true`. | `"false"` |
| `ui.httpsRedirect` | Redirect HTTP to HTTPS inside the app. Kept `false`; TLS terminates at the ingress. | `"false"` |
| `ui.oidc.clientId` | Keycloak client ID for the UI's own `Data-Egress-UI` client. | `Data-Egress-UI` |
| `ui.oidc.remoteSignOutPath` | Path the OIDC middleware listens on for a Keycloak-initiated sign-out callback. | `/SignOut` |
| `ui.oidc.signedOutRedirectUri` | Where the browser lands after that sign-out completes. | `/` |
| `ui.oidc.tokenExpiredAddress` | Empty computes `https://egress.<global.ingress.host>/Account/LoginAfterTokenExpired`. Only appears in a log message. | `""` |
| `ui.oidc.autoTrustKeycloakCert` | Trust Keycloak's certificate without validation. Keep `false`; use `global.trustClusterCa` instead. | `"false"` |
| `ui.oidc.validIssuer` | Expected token issuer override. Empty uses the Authority. | `""` |
| `ui.oidc.validAudience` | Expected token audience override. Empty skips the check. | `""` |
| `ui.oidc.proxy` | The UI sits behind an egress proxy when calling Keycloak. | `"false"` |
| `ui.oidc.proxyAddresUrl` | Proxy address, used when `ui.oidc.proxy` is `"true"`. | `""` |
| `ui.oidc.bypassProxy` | Comma-separated proxy bypass list. | `""` |
| `ui.s3ConsoleUrl` | Public RustFS console URL shown to users, for a display-only link. Unrelated to `api.s3Url`. | `http://localhost:9003` |
| `ui.s3BucketPath` | RustFS console path template appended to a bucket name. | `/rustfs/console/browser/?bucket=` |
| `ui.helpdeskUrl` | Helpdesk link shown in the UI. | `https://ukserp.atlassian.net/servicedesk/customer/portal/3` |
| `ui.extraEnv` | Extra environment variables. | `[]` |
