{{/*
Expand the name of the chart.
*/}}
{{- define "agent.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "agent.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "agent.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels - egress
*/}}
{{- define "agent.egressApiLabels" -}}
helm.sh/chart: {{ include "agent.chart" . }}
{{ include "agent.egressApiSelectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Common labels - egress
*/}}
{{- define "agent.egressUiLabels" -}}
helm.sh/chart: {{ include "agent.chart" . }}
{{ include "agent.egressUiSelectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Common labels - tre
*/}}
{{- define "agent.treApiLabels" -}}
helm.sh/chart: {{ include "agent.chart" . }}
{{ include "agent.treApiSelectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Common labels - tre
*/}}
{{- define "agent.treUiLabels" -}}
helm.sh/chart: {{ include "agent.chart" . }}
{{ include "agent.treUiSelectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Egress selector labels
*/}}
{{- define "agent.egressApiSelectorLabels" -}}
app.kubernetes.io/name: {{ include "agent.name" . }}-egress-api
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Egress selector labels
*/}}
{{- define "agent.egressUiSelectorLabels" -}}
app.kubernetes.io/name: {{ include "agent.name" . }}-egress-ui
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Tre selector labels
*/}}
{{- define "agent.treApiSelectorLabels" -}}
app.kubernetes.io/name: {{ include "agent.name" . }}-tre-api
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Tre selector labels
*/}}
{{- define "agent.treUiSelectorLabels" -}}
app.kubernetes.io/name: {{ include "agent.name" . }}-tre-ui
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Create the name of the egress service account to use
*/}}
{{- define "agent.egressServiceAccountName" -}}
{{- if .Values.egress.serviceAccount.create }}
{{- default (include "agent.fullname" .) .Values.egress.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.egress.serviceAccount.name }}
{{- end }}
{{- end }}

{{/*
Create the name of the tre service account to use
*/}}
{{- define "agent.treServiceAccountName" -}}
{{- if .Values.tre.serviceAccount.create }}
{{- default (include "agent.fullname" .) .Values.tre.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.tre.serviceAccount.name }}
{{- end }}
{{- end }}

{{/*
Create env var settings that are common to all containers
*/}}
{{- define "agent.commonEnvVars" -}}
{{- if .Values.seq.enabled }}
- name: Serilog__SeqServerUrl
  value: "{{ .Values.seq.url }}"
{{- end }}
- name: DataEgressAPISettings__Address
  value: "http://{{ include "agent.fullname" . }}-egress-api:{{ .Values.egress.api.service.port }}"
- name: RabbitMQ__HostAddress
  value: "{{ .Values.rabbitmq.host }}"
- name: RabbitMQ__Username
  value: "{{ .Values.rabbitmq.username }}"
- name: RabbitMQ__Password
  valueFrom:
    secretKeyRef:
      name: {{ .Values.rabbitmq.passwordSecret.name }}
      key: {{ .Values.rabbitmq.passwordSecret.key }}
{{- end }}

{{/*
Create the common data egress Keycloak settings
*/}}
{{- define "agent.egressKeycloakSettings" -}}
- name: DataEgressKeyCloakSettings__Authority
  value: "{{ .Values.egress.config.keycloak.baseUrl}}/realms/{{ .Values.egress.config.keycloak.realm }}"
- name: DataEgressKeyCloakSettings__MetadataAddress
  value: "{{ .Values.egress.config.keycloak.baseUrl}}/realms/{{ .Values.egress.config.keycloak.realm }}/.well-known/openid-configuration"
- name: DataEgressKeyCloakSettings__BaseUrl
  value: "{{ .Values.egress.config.keycloak.baseUrl}}/realms/{{ .Values.egress.config.keycloak.realm }}"
- name: DataEgressKeyCloakSettings__ClientId
  valueFrom:
    secretKeyRef:
      name: {{ .Values.egress.config.keycloak.clientAuthSecret.name }}
      key: {{ .Values.egress.config.keycloak.clientAuthSecret.idKey }}
- name: DataEgressKeyCloakSettings__ClientSecret
  valueFrom:
    secretKeyRef:
      name: {{ .Values.egress.config.keycloak.clientAuthSecret.name }}
      key: {{ .Values.egress.config.keycloak.clientAuthSecret.secretKey }}
- name: DataEgressKeyCloakSettings__ValidAudiences
  value: "{{ .Values.egress.config.keycloak.validAudiences }}"
- name: DataEgressKeyCloakSettings__TokenExpiredAddress
{{- if .Values.egress.ui.ingress.enabled }}
  value: "https://{{ .Values.egress.ui.ingress.host }}/Account/LoginAfterTokenExpired"
{{- else }}
  value: "{{ .Values.egress.config.keycloak.tokenExpiredAddress }}"
{{- end }}
- name: DataEgressKeyCloakSettings__UseRedirectURL
  value: "{{ .Values.egress.config.keycloak.redirect.enabled }}"
{{- if .Values.egress.config.keycloak.redirect.enabled }}
- name: DataEgressKeyCloakSettings__RedirectURL
  value: "{{ .Values.egress.config.keycloak.redirect.url }}"
{{- end }}
- name: DataEgressKeyCloakSettings__Proxy
  value: "{{ .Values.egress.config.keycloak.proxy.enabled }}"
{{- if .Values.egress.config.keycloak.proxy.enabled }}
- name: DataEgressKeyCloakSettings__ProxyAddresURL
  value: "{{ .Values.egress.config.keycloak.proxy.url }}"
- name: DataEgressKeyCloakSettings__BypassProxy
  value: "{{ .Values.egress.config.keycloak.proxy.bypass }}"
{{- end }}
{{- end }}

{{/*
Create the common TRE Keycloak settings
*/}}
{{- define "agent.treKeycloakSettings" -}}
- name: TreKeyCloakSettings__Authority
  value: "{{ .Values.tre.config.keycloak.baseUrl}}/realms/{{ .Values.tre.config.keycloak.realm }}"
- name: TreKeyCloakSettings__MetadataAddress
  value: "{{ .Values.tre.config.keycloak.baseUrl}}/realms/{{ .Values.tre.config.keycloak.realm }}/.well-known/openid-configuration"
- name: TreKeyCloakSettings__BaseUrl
  value: "{{ .Values.tre.config.keycloak.baseUrl}}/realms/{{ .Values.tre.config.keycloak.realm }}"
- name: TreKeyCloakSettings__ClientId
  valueFrom:
    secretKeyRef:
      name: {{ .Values.tre.config.keycloak.clientAuthSecret.name }}
      key: {{ .Values.tre.config.keycloak.clientAuthSecret.idKey }}
- name: TreKeyCloakSettings__ClientSecret
  valueFrom:
    secretKeyRef:
      name: {{ .Values.tre.config.keycloak.clientAuthSecret.name }}
      key: {{ .Values.tre.config.keycloak.clientAuthSecret.secretKey }}
- name: TreKeyCloakSettings__ValidAudiences
  value: "{{ .Values.tre.config.keycloak.validAudiences }}"
- name: TreKeyCloakSettings__TokenExpiredAddress
{{- if .Values.tre.ui.ingress.enabled }}
  value: "https://{{ .Values.tre.ui.ingress.host }}/Account/LoginAfterTokenExpired"
{{- else }}
  value: "{{ .Values.tre.config.keycloak.tokenExpiredAddress }}"
{{- end }}
- name: TreKeyCloakSettings__AccountManagementURL
  value: "{{ .Values.tre.config.keycloak.baseUrl}}/realms/{{ .Values.tre.config.keycloak.realm }}/account"
- name: TreKeyCloakSettings__UseRedirectURL
  value: "{{ .Values.tre.config.keycloak.redirect.enabled }}"
{{- if .Values.tre.config.keycloak.redirect.enabled }}
- name: TreKeyCloakSettings__RedirectURL
  value: "{{ .Values.tre.config.keycloak.redirect.url }}"
{{- end }}
- name: TreKeyCloakSettings__Proxy
  value: "{{ .Values.tre.config.keycloak.proxy.enabled }}"
{{- if .Values.tre.config.keycloak.proxy.enabled }}
- name: TreKeyCloakSettings__ProxyAddresURL
  value: "{{ .Values.tre.config.keycloak.proxy.url }}"
- name: TreKeyCloakSettings__BypassProxy
  value: "{{ .Values.tre.config.keycloak.proxy.bypass }}"
{{- end }}
{{- end }}

{{/*
Create the common Submission Keycloak settings
*/}}
{{- define "agent.submissionKeycloakSettings" -}}
- name: SubmissionKeyCloakSettings__Authority
  value: "{{ .Values.submission.config.keycloak.baseUrl}}/realms/{{ .Values.submission.config.keycloak.realm }}"
- name: SubmissionKeyCloakSettings__MetadataAddress
  value: "{{ .Values.submission.config.keycloak.baseUrl}}/realms/{{ .Values.submission.config.keycloak.realm }}/.well-known/openid-configuration"
- name: SubmissionKeyCloakSettings__BaseUrl
  value: "{{ .Values.submission.config.keycloak.baseUrl}}/realms/{{ .Values.submission.config.keycloak.realm }}"
- name: SubmissionKeyCloakSettings__ClientId
  valueFrom:
    secretKeyRef:
      name: {{ .Values.submission.config.keycloak.clientAuthSecret.name }}
      key: {{ .Values.submission.config.keycloak.clientAuthSecret.idKey }}
- name: SubmissionKeyCloakSettings__ClientSecret
  valueFrom:
    secretKeyRef:
      name: {{ .Values.submission.config.keycloak.clientAuthSecret.name }}
      key: {{ .Values.submission.config.keycloak.clientAuthSecret.secretKey }}
- name: SubmissionKeyCloakSettings__ValidAudiences
  value: "{{ .Values.submission.config.keycloak.validAudiences }}"
- name: SubmissionKeyCloakSettings__TokenExpiredAddress
  value: "{{ .Values.submission.config.keycloak.tokenExpiredAddress }}"
- name: SubmissionKeyCloakSettings__AccountManagementURL
  value: "{{ .Values.submission.config.keycloak.baseUrl}}/realms/{{ .Values.submission.config.keycloak.realm }}/account"
- name: SubmissionKeyCloakSettings__UseRedirectURL
  value: "{{ .Values.submission.config.keycloak.redirect.enabled }}"
{{- if .Values.submission.config.keycloak.redirect.enabled }}
- name: SubmissionKeyCloakSettings__RedirectURL
  value: "{{ .Values.submission.config.keycloak.redirect.url }}"
{{- end }}
- name: SubmissionKeyCloakSettings__Proxy
  value: "{{ .Values.submission.config.keycloak.proxy.enabled }}"
{{- if .Values.submission.config.keycloak.proxy.enabled }}
- name: SubmissionKeyCloakSettings__ProxyAddresURL
  value: "{{ .Values.submission.config.keycloak.proxy.url }}"
- name: SubmissionKeyCloakSettings__BypassProxy
  value: "{{ .Values.submission.config.keycloak.proxy.bypass }}"
{{- end }}
{{- end }}


{{/*
Create the common Agent settings
*/}}
{{- define "agent.agentSettings" -}}
- name: AgentSettings__UseTESK
  value: "{{ .Values.tre.config.agent.tesk.enabled }}"
{{- if .Values.tre.config.agent.tesk.enabled }}
- name: AgentSettings__TESKAPIURL
  value: "{{ .Values.tre.config.agent.tesk.apiUrl }}"
- name: AgentSettings__TESKOutputBucketPrefix
  value: "{{ .Values.tre.config.agent.tesk.outputBucketPrefix }}"
{{- end }}
- name: AgentSettings__UseHutch
  value: "{{ .Values.tre.config.agent.hutch.enabled }}"
{{- if .Values.tre.config.agent.hutch.enabled }}
- name: Hutch__APIAddress
  value: "{{ .Values.tre.config.agent.hutch.apiUrl }}"
- name: Hutch__DbServer
  value: "{{ .Values.tre.config.agent.hutch.database.host }}"
- name: Hutch__DbName
  value: "{{ .Values.tre.config.agent.hutch.database.name }}"
- name: Hutch__DbPort
  value: "{{ .Values.tre.config.agent.hutch.database.port }}" 
- name: IgnoreHutchSSL
  value: "{{ .Values.tre.config.agent.hutch.ignoreSsl }}"
{{- end }}
- name: AgentSettings__UseRabbit
  value: "{{ .Values.tre.config.agent.rabbit.enabled }}"
- name: AgentSettings__URLHasuraToAdd
  value: "{{ .Values.tre.config.agent.hasura.externalAddress }}"
- name: AgentSettings__ImageNameToAddToToken
  value: "{{ .Values.tre.config.agent.image }}"
- name: AgentSettings__Proxy
  value: "{{ .Values.tre.config.agent.proxy.enabled }}"
{{- if .Values.tre.config.agent.proxy.enabled }}
- name: AgentSettings__ProxyAddresURL
  value: "{{ .Values.tre.config.agent.proxy.url }}"
{{- end }}
- name: HasuraSettings__HasuraURL
  value: "{{ .Values.tre.config.agent.hasura.internalAddress }}"
- name: HasuraSettings__HasuraAdminSecret
  value: "{{ .Values.tre.config.agent.hasura.adminPassword }}"
{{- end }}

{{/* 
Create TRE minio settings 
*/}}
{{- define "agent.treMinioSettings" -}}
- name: MinioTRESettings__Url
  value: "{{ .Values.tre.config.minio.url }}"
- name: MinioTRESettings__AccessKey
  valueFrom:
    secretKeyRef:
      name: {{ .Values.tre.config.minio.authSecret.name }}
      key: {{ .Values.tre.config.minio.authSecret.accessKeyRef }}
- name: MinioTRESettings__SecretKey
  valueFrom:
    secretKeyRef:
      name: {{ .Values.tre.config.minio.authSecret.name }}
      key: {{ .Values.tre.config.minio.authSecret.secretKeyRef }}
- name: MinioTRESettings__BucketName
  value: "{{ .Values.tre.config.minio.bucketName }}"
- name: MinioTRESettings__AdminConsole
  value: "{{ .Values.tre.config.minio.adminConsole }}"
- name: MinioTRESettings__HutchURLOverride
  value: "{{ .Values.tre.config.agent.hutch.minioUrlOverride }}"
- name: MinioTRESettings__AWSRegion
  value: "us-east-1"
{{- end }}

{{/* 
Create submission minio settings 
*/}}
{{- define "agent.submissionMinioSettings" -}}
- name: MinioSubSettings__Url
  value: "{{ .Values.submission.config.minio.url }}"
- name: MinioSubSettings__AccessKey
  valueFrom:
    secretKeyRef:
      name: {{ .Values.submission.config.minio.authSecret.name }}
      key: {{ .Values.submission.config.minio.authSecret.accessKeyRef }}
- name: MinioSubSettings__SecretKey
  valueFrom:
    secretKeyRef:
      name: {{ .Values.submission.config.minio.authSecret.name }}
      key: {{ .Values.submission.config.minio.authSecret.secretKeyRef }}
- name: MinioSubSettings__BucketName
  value: "{{ .Values.submission.config.minio.bucketName }}"
- name: MinioSubSettings__AdminConsole
  value: "{{ .Values.submission.config.minio.adminConsole }}"
- name: MinioSubSettings__AWSRegion
  value: "us-east-1"
{{- end }}