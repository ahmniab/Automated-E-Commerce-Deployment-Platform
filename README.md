# Automated-E-Commerce-Deployment-Platform-
## Google Drive
> [https://drive.google.com/drive/folders/1_bfdHB6V68hbIReWgEb5oiZizWFRzIpw?usp=sharing](https://drive.google.com/drive/folders/1_bfdHB6V68hbIReWgEb5oiZizWFRzIpw?usp=sharing)

## Team members
- Ahmed Nabil
- Fady Yacoub
- Kirollos Joseph
- Ahmed Temsah
- AbdAllah Amr
- toni ihab

## Plan

|**Period**|**Time in Days**|**Task & Deliverables**|
|---|---|---|
|**16-2 -> 28-2**|13 Days|**Phase 1: Version Control & AWS Foundations**<br><br>  <br><br>• Setup the base Git repository for code and scripts (Deliverable 9).<br><br>  <br><br>• Learn core AWS networking and compute concepts: EC2 (virtual servers), RDS (managed databases), S3 (object storage), and ELB (load balancing).<br><br>  <br><br>• Manually provision these resources once to understand how they work together before automating them.|
|**1-3 -> 15-3**|15 Days|**Phase 2: Infrastructure as Code (Terraform)**<br><br>  <br><br>• Learn Terraform state management, providers, and modules.<br><br>  <br><br>• Write and test Terraform scripts to automatically provision the AWS environment (EC2, RDS, S3, ELB) (Deliverable 7).<br><br>  <br><br>• Ensure the infrastructure can be spun up and torn down completely via code.|
|**16-3 -> 30-3**|15 Days|**Phase 3: Configuration Management (Ansible)**<br><br>  <br><br>• Learn Ansible playbooks, inventory files, and roles.<br><br>  <br><br>• Write Ansible scripts for baseline server and configuration setup on the EC2 instances provisioned by Terraform (Deliverable 4).<br><br>  <br><br>• Automate the installation of required base packages (like Docker, monitoring agents) across multiple nodes.|
|**31-3 -> 14-4**|15 Days|**Phase 4: Containerization (Docker)**<br><br>  <br><br>• Study Docker images, containers, Dockerfiles, and multi-stage builds.<br><br>  <br><br>• Containerize the sample e-commerce microservices (web frontend, database, payment, search) (Deliverable 1).<br><br>  <br><br>• Use Docker Compose locally to ensure all microservices communicate correctly over a dedicated Docker network.|
|**15-4 -> 10-5**|26 Days|**Phase 5: Container Orchestration (Kubernetes)**<br><br>  <br><br>• _Deep Dive Phase:_ Learn Kubernetes architecture, Pods, Deployments, Services, and ConfigMaps/Secrets.<br><br>  <br><br>• Write Kubernetes manifest files for the e-commerce microservices.<br><br>  <br><br>• Implement the Horizontal Pod Autoscaler (HPA) for auto-scaling based on CPU/Memory usage (Deliverable 3).|
|**11-5 -> 25-5**|15 Days|**Phase 6: Routing & Reverse Proxy (Nginx)**<br><br>  <br><br>• Learn Nginx configuration, reverse proxying, and load balancing.<br><br>  <br><br>• Set up Nginx to handle incoming traffic and route it to the appropriate Kubernetes services or microservice backends (Deliverable 6).<br><br>  <br><br>• _Optional:_ Implement Nginx as a Kubernetes Ingress Controller.|
|**26-5 -> 15-6**|21 Days|**Phase 7: Continuous Integration & Deployment (Jenkins)**<br><br>  <br><br>• Set up a Jenkins server and learn to write Jenkinsfiles (Declarative Pipelines).<br><br>  <br><br>• Build CI/CD pipelines to automatically fetch code from Git, build Docker images, push to a registry, and deploy updates to Kubernetes (Deliverable 2).<br><br>  <br><br>• Implement and test automated rollback strategies within the pipeline if a deployment fails health checks (Deliverable 8).|
|**16-6 -> 30-6**|15 Days|**Phase 8: Monitoring & Observability (Prometheus & Grafana)**<br><br>  <br><br>• Learn how Prometheus scrapes metrics and how Grafana visualizes them.<br><br>  <br><br>• Deploy Prometheus and Grafana into your infrastructure.<br><br>  <br><br>• Configure Prometheus to monitor the microservices, Nginx, and Kubernetes cluster.<br><br>  <br><br>• Build Grafana dashboards for real-time service monitoring (Deliverable 5).|
|**1-7 -> 15-7**|15 Days|**Phase 9: Full Integration & Testing**<br><br>  <br><br>• Connect all the pieces: Use Terraform to build the infra, Ansible to configure it, Jenkins to deploy the Kubernetes manifests, and Grafana to monitor the result.<br><br>  <br><br>• Conduct load testing to trigger Kubernetes auto-scaling.<br><br>  <br><br>• Intentionally fail a deployment to watch the automated rollback succeed.|
|**16-7 -> 23-7**|8 Days|**Phase 10: Buffer, Polish & Documentation**<br><br>  <br><br>• Use this week to fix any lingering bugs or pipeline bottlenecks.<br><br>  <br><br>• Clean up the Git repository, ensure all secrets are hidden, and write a comprehensive `README.md` documenting the architecture and deployment steps.|
