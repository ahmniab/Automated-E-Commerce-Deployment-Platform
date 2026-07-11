terraform {
  required_providers {
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.30"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~> 2.15"
    }
  }
}

provider "kubernetes" {
  config_path    = "~/.kube/config"
  config_context = "docker-desktop"
}

provider "helm" {
  kubernetes {
    config_path    = "~/.kube/config"
    config_context = "docker-desktop"
  }
}

resource "kubernetes_namespace" "monitoring" {
  metadata {
    name = "monitoring-tf"
  }
}

resource "helm_release" "prometheus_stack" {
  name       = "kube-prometheus-stack"
  repository = "https://prometheus-community.github.io/helm-charts"
  chart      = "kube-prometheus-stack"
  namespace  = kubernetes_namespace.monitoring.metadata[0].name
  version    = "61.3.0"
  timeout    = 1200

  values = [
    <<-EOT
    additionalPrometheusRulesMap:
      custom-rules:
        groups:
          - name: custom-resource-alerts
            rules:
              # 1. Node CPU Alert (> 80%)
              - alert: NodeCpuUsageHigh
                expr: (1 - avg(rate(node_cpu_seconds_total{mode="idle"}[5m])) by (instance)) * 100 > 80
                for: 2m
                labels:
                  severity: warning
                annotations:
                  summary: "High CPU usage on node {{ $labels.instance }}"
                  description: "Node CPU utilization is at {{ printf \"%.2f\" $value }}% (above 80%) for more than 2 minutes."

              # 2. Node Memory Alert (> 90%)
              - alert: NodeMemoryUsageHigh
                expr: (1 - (node_memory_MemAvailable_bytes / node_memory_MemTotal_bytes)) * 100 > 90
                for: 2m
                labels:
                  severity: warning
                annotations:
                  summary: "High Memory usage on node {{ $labels.instance }}"
                  description: "Node memory utilization is at {{ printf \"%.2f\" $value }}% (above 90%) for more than 2 minutes."

              # 3. Pod CPU Alert (> 80% of limit)
              - alert: PodCpuUsageHigh
                expr: (sum(rate(container_cpu_usage_seconds_total{container!=""}[5m])) by (pod, namespace) / sum(kube_pod_container_resource_limits{resource="cpu",container!=""}) by (pod, namespace)) * 100 > 80
                for: 2m
                labels:
                  severity: warning
                annotations:
                  summary: "High CPU usage on pod {{ $labels.namespace }}/{{ $labels.pod }}"
                  description: "Pod {{ $labels.pod }} is using {{ printf \"%.2f\" $value }}% CPU limit (above 80%) for more than 2 minutes."

              # 4. Pod Memory Alert (> 90% of limit)
              - alert: PodMemoryUsageHigh
                expr: (sum(container_memory_working_set_bytes{container!=""}) by (pod, namespace) / sum(kube_pod_container_resource_limits{resource="memory",container!=""}) by (pod, namespace)) * 100 > 90
                for: 2m
                labels:
                  severity: warning
                annotations:
                  summary: "High Memory usage on pod {{ $labels.namespace }}/{{ $labels.pod }}"
                  description: "Pod {{ $labels.pod }} is using {{ printf \"%.2f\" $value }}% memory limit (above 90%) for more than 2 minutes."

    prometheus:
      prometheusSpec:
        podMonitorSelectorNilUsesHelmValues: false
        serviceMonitorSelectorNilUsesHelmValues: false
        additionalScrapeConfigs:
          - job_name: 'kubernetes-pods'
            kubernetes_sd_configs:
              - role: pod
            relabel_configs:
              - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_scrape]
                action: keep
                regex: "true"
              - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_path]
                action: replace
                target_label: __metrics_path__
                regex: (.+)
              - source_labels: [__address__, __meta_kubernetes_pod_annotation_prometheus_io_port]
                action: replace
                regex: ([^:]+)(?::\d+)?;(\d+)
                replacement: $1:$2
                target_label: __address__
              - action: labelmap
                regex: __meta_kubernetes_pod_label_(.+)
              - source_labels: [__meta_kubernetes_namespace]
                action: replace
                target_label: kubernetes_namespace
              - source_labels: [__meta_kubernetes_pod_name]
                action: replace
                target_label: kubernetes_pod_name

    alertmanager:
      config:
        global:
          resolve_timeout: 5m
        route:
          group_by: ['alertname']
          group_wait: 30s
          group_interval: 5m
          repeat_interval: 12h
          receiver: 'telegram-receiver'
        receivers:
        - name: 'telegram-receiver'
          telegram_configs:
          - bot_token: '8326101105:AAHP1SQ-vVVYFZsZhCNVavIU2OVD1oqPcWQ'
            chat_id: -5315590669
            send_resolved: true
            parse_mode: 'HTML'
            message: |
              <b>[{{ .Status | toUpper }}{{ if eq .Status "firing" }}🔥{{ else }}✅{{ end }}] {{ .CommonLabels.alertname }}</b>
              <b>Severity:</b> <code>{{ .CommonLabels.severity }}</code>
              <b>Summary:</b> {{ .CommonAnnotations.summary }}
              <b>Description:</b> {{ .CommonAnnotations.description }}
        - name: 'null'
    EOT
  ]

  set {
    name  = "grafana.adminPassword"
    value = "admin"
  }
}
