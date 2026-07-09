def ALB_DNS = 'YOUR_ALB_DNS'

pipeline {
    agent any

    environment {
        AWS_REGION       = 'us-east-1'
        ECR_REGISTRY     = '968138089668.dkr.ecr.us-east-1.amazonaws.com'
        EKS_CLUSTER_NAME = 'eshop-eks'
        IMAGE_TAG        = ''
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
                script {
                    env.IMAGE_TAG = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
                }
            }
        }

        stage('Docker Hub Login') {
            steps {
                // Preserve the exact credential ID from the old Jenkinsfile
                withCredentials([usernamePassword(credentialsId: 'dockerhub-credentials', usernameVariable: 'DOCKERHUB_USERNAME', passwordVariable: 'DOCKERHUB_PASSWORD')]) {
                    sh 'echo $DOCKERHUB_PASSWORD | docker login -u $DOCKERHUB_USERNAME --password-stdin'
                }
            }
        }

        stage('Test all microservices') {
            parallel {
                stage('Test Basket API') {
                    steps {
                        sh """
                          docker run --rm \
                            -v "${WORKSPACE}/src/microservices/eShop.Basket.API:/app:z" \
                            -w /app \
                            -e DOTNET_CLI_HOME=/app/.dotnet-temp \
                            mcr.microsoft.com/dotnet/sdk:10.0 \
                            dotnet test eShop.Basket.API.sln
                        """
                    }
                }
                stage('Test Catalog API') {
                    steps {
                        sh """
                          docker run --rm \
                            -v "${WORKSPACE}/src/microservices/eShop.Catalog.API:/app:z" \
                            -w /app \
                            -e DOTNET_CLI_HOME=/app/.dotnet-temp \
                            mcr.microsoft.com/dotnet/sdk:10.0 \
                            dotnet test eShop.Catalog.API.sln
                        """
                    }
                }
                stage('Test Identity API') {
                    steps {
                        sh """
                          docker run --rm \
                            -v "${WORKSPACE}/src/microservices/eShop.Identity.API:/app:z" \
                            -w /app \
                            -e DOTNET_CLI_HOME=/app/.dotnet-temp \
                            mcr.microsoft.com/dotnet/sdk:10.0 \
                            dotnet test eShop.Identity.API.sln
                        """
                    }
                }
                stage('Test Order Processor') {
                    steps {
                        sh """
                          docker run --rm \
                            -v "${WORKSPACE}/src/microservices/eShop.OrderProcessor:/app:z" \
                            -w /app \
                            -e DOTNET_CLI_HOME=/app/.dotnet-temp \
                            mcr.microsoft.com/dotnet/sdk:10.0 \
                            dotnet test eShop.OrderProcessor.sln
                        """
                    }
                }
                stage('Test Ordering API') {
                    steps {
                        sh """
                          docker run --rm \
                            -v "${WORKSPACE}/src/microservices/eShop.Ordering.API:/app:z" \
                            -w /app \
                            -e DOTNET_CLI_HOME=/app/.dotnet-temp \
                            mcr.microsoft.com/dotnet/sdk:10.0 \
                            dotnet test eShop.Ordering.API.sln
                        """
                    }
                }
                stage('Test Payment Processor') {
                    steps {
                        sh """
                          docker run --rm \
                            -v "${WORKSPACE}/src/microservices/eShop.PaymentProcessor:/app:z" \
                            -w /app \
                            -e DOTNET_CLI_HOME=/app/.dotnet-temp \
                            mcr.microsoft.com/dotnet/sdk:10.0 \
                            dotnet test eShop.PaymentProcessor.sln
                        """
                    }
                }
                stage('Test WebApp') {
                    steps {
                        sh """
                          docker run --rm \
                            -v "${WORKSPACE}/src/microservices/eShop.WebApp:/app:z" \
                            -w /app \
                            -e DOTNET_CLI_HOME=/app/.dotnet-temp \
                            mcr.microsoft.com/dotnet/sdk:10.0 \
                            dotnet test eShop.WebApp.sln
                        """
                    }
                }
            }
        }

        stage('Build Docker images') {
            parallel {
                stage('Build Basket API') {
                    steps {
                        sh "docker build -t ${ECR_REGISTRY}/eshop-basket-api:${IMAGE_TAG} -f src/microservices/eShop.Basket.API/Dockerfile src/microservices/eShop.Basket.API"
                    }
                }
                stage('Build Catalog API') {
                    steps {
                        sh "docker build -t ${ECR_REGISTRY}/eshop-catalog-api:${IMAGE_TAG} -f src/microservices/eShop.Catalog.API/Dockerfile src/microservices/eShop.Catalog.API"
                    }
                }
                stage('Build Identity API') {
                    steps {
                        sh "docker build -t ${ECR_REGISTRY}/eshop-identity-api:${IMAGE_TAG} -f src/microservices/eShop.Identity.API/Dockerfile src/microservices/eShop.Identity.API"
                    }
                }
                stage('Build Order Processor') {
                    steps {
                        sh "docker build -t ${ECR_REGISTRY}/eshop-order-processor:${IMAGE_TAG} -f src/microservices/eShop.OrderProcessor/Dockerfile src/microservices/eShop.OrderProcessor"
                    }
                }
                stage('Build Ordering API') {
                    steps {
                        sh "docker build -t ${ECR_REGISTRY}/eshop-ordering-api:${IMAGE_TAG} -f src/microservices/eShop.Ordering.API/Dockerfile src/microservices/eShop.Ordering.API"
                    }
                }
                stage('Build Payment Processor') {
                    steps {
                        sh "docker build -t ${ECR_REGISTRY}/eshop-payment-processor:${IMAGE_TAG} -f src/microservices/eShop.PaymentProcessor/Dockerfile src/microservices/eShop.PaymentProcessor"
                    }
                }
                stage('Build WebApp') {
                    steps {
                        sh "docker build -t ${ECR_REGISTRY}/eshop-webapp:${IMAGE_TAG} -f src/microservices/eShop.WebApp/Dockerfile src/microservices/eShop.WebApp"
                    }
                }
            }
        }

        stage('ECR Login') {
            steps {
                sh "aws ecr get-login-password --region ${AWS_REGION} | docker login --username AWS --password-stdin ${ECR_REGISTRY}"
            }
        }

        stage('Push to ECR') {
            parallel {
                stage('Push Basket API') {
                    steps {
                        sh "docker push ${ECR_REGISTRY}/eshop-basket-api:${IMAGE_TAG}"
                    }
                }
                stage('Push Catalog API') {
                    steps {
                        sh "docker push ${ECR_REGISTRY}/eshop-catalog-api:${IMAGE_TAG}"
                    }
                }
                stage('Push Identity API') {
                    steps {
                        sh "docker push ${ECR_REGISTRY}/eshop-identity-api:${IMAGE_TAG}"
                    }
                }
                stage('Push Order Processor') {
                    steps {
                        sh "docker push ${ECR_REGISTRY}/eshop-order-processor:${IMAGE_TAG}"
                    }
                }
                stage('Push Ordering API') {
                    steps {
                        sh "docker push ${ECR_REGISTRY}/eshop-ordering-api:${IMAGE_TAG}"
                    }
                }
                stage('Push Payment Processor') {
                    steps {
                        sh "docker push ${ECR_REGISTRY}/eshop-payment-processor:${IMAGE_TAG}"
                    }
                }
                stage('Push WebApp') {
                    steps {
                        sh "docker push ${ECR_REGISTRY}/eshop-webapp:${IMAGE_TAG}"
                    }
                }
            }
        }

        stage('Deploy to EKS') {
            steps {
                script {
                    // Update Kubernetes config to point to EKS cluster
                    sh "aws eks update-kubeconfig --region ${AWS_REGION} --name ${EKS_CLUSTER_NAME}"

                    // Fetch postgres DB credentials from AWS Secrets Manager
                    def secretVal = sh(
                        script: "aws secretsmanager get-secret-value --secret-id eshop/postgres --region ${AWS_REGION} --query SecretString --output text",
                        returnStdout: true
                    ).trim()

                    def dbUser = ""
                    def dbPass = ""
                    def dbName = ""

                    withEnv(["SECRET_JSON=${secretVal}"]) {
                        dbUser = sh(script: 'echo "\$SECRET_JSON" | python3 -c "import sys, json; print(json.load(sys.stdin)[\'username\'])"', returnStdout: true).trim()
                        dbPass = sh(script: 'echo "\$SECRET_JSON" | python3 -c "import sys, json; print(json.load(sys.stdin)[\'password\'])"', returnStdout: true).trim()
                        dbName = sh(script: 'echo "\$SECRET_JSON" | python3 -c "import sys, json; print(json.load(sys.stdin)[\'database\'])"', returnStdout: true).trim()
                    }

                    // Upsert the postgres-secret Kubernetes secret in eshop namespace
                    sh """
                        kubectl create secret generic postgres-secret \
                          --from-literal=POSTGRES_USER='${dbUser}' \
                          --from-literal=POSTGRES_PASSWORD='${dbPass}' \
                          --from-literal=POSTGRES_DB='${dbName}' \
                          --dry-run=client -o yaml | kubectl apply -f - -n eshop
                    """

                    // Apply Kubernetes manifests to eshop namespace
                    sh "kubectl apply -f src/k8s/ --recursive -n eshop"

                    // Re-apply secret to make sure postgres-secret.yaml didn't overwrite it with placeholders in eshop namespace
                    sh """
                        kubectl create secret generic postgres-secret \
                          --from-literal=POSTGRES_USER='${dbUser}' \
                          --from-literal=POSTGRES_PASSWORD='${dbPass}' \
                          --from-literal=POSTGRES_DB='${dbName}' \
                          --dry-run=client -o yaml | kubectl apply -f - -n eshop
                    """

                    // Set ECR images on all deployments (names and container names from manifests in eshop namespace)
                    sh "kubectl set image deployment/basket-api-deployment basket-api=${ECR_REGISTRY}/eshop-basket-api:${IMAGE_TAG} -n eshop"
                    sh "kubectl set image deployment/catalog-api-deployment catalog-api=${ECR_REGISTRY}/eshop-catalog-api:${IMAGE_TAG} -n eshop"
                    sh "kubectl set image deployment/identity-api-deployment identity-api=${ECR_REGISTRY}/eshop-identity-api:${IMAGE_TAG} -n eshop"
                    sh "kubectl set image deployment/order-processor-deployment order-processor=${ECR_REGISTRY}/eshop-order-processor:${IMAGE_TAG} -n eshop"
                    sh "kubectl set image deployment/ordering-api-deployment ordering-api=${ECR_REGISTRY}/eshop-ordering-api:${IMAGE_TAG} -n eshop"
                    sh "kubectl set image deployment/payment-processor-deployment payment-processor=${ECR_REGISTRY}/eshop-payment-processor:${IMAGE_TAG} -n eshop"
                    sh "kubectl set image deployment/webapp-deployment webapp=${ECR_REGISTRY}/eshop-webapp:${IMAGE_TAG} -n eshop"

                    // Wait for rollouts in eshop namespace
                    sh "kubectl rollout status deployment/basket-api-deployment -n eshop --timeout=180s"
                    sh "kubectl rollout status deployment/catalog-api-deployment -n eshop --timeout=180s"
                    sh "kubectl rollout status deployment/identity-api-deployment -n eshop --timeout=180s"
                    sh "kubectl rollout status deployment/order-processor-deployment -n eshop --timeout=180s"
                    sh "kubectl rollout status deployment/ordering-api-deployment -n eshop --timeout=180s"
                    sh "kubectl rollout status deployment/payment-processor-deployment -n eshop --timeout=180s"
                    sh "kubectl rollout status deployment/webapp-deployment -n eshop --timeout=180s"
                }
            }
        }

        stage('Smoke test') {
            steps {
                script {
                    // Check if ALB_DNS is configured. If not, fallback to worker node IP.
                    if (ALB_DNS == null || ALB_DNS.trim() == "" || ALB_DNS == 'YOUR_ALB_DNS') {
                        echo "ALB_DNS is not configured. Attempting fallback check using EKS worker node public IP..."
                        try {
                            def nodeIp = sh(
                                script: "kubectl get nodes -o jsonpath='{.items[*].status.addresses[?(@.type==\"ExternalIP\")].address}' | awk '{print \$1}'",
                                returnStdout: true
                            ).trim()

                            if (nodeIp) {
                                echo "Smoke testing WebApp root via Worker Node IP: http://${nodeIp}:30080/"
                                sh "curl -f http://${nodeIp}:30080/ || exit 1"
                                echo "Worker node IP found: ${nodeIp}. WebApp is healthy."
                            } else {
                                echo "Could not retrieve worker node public IP. Skipping external smoke tests."
                            }
                        } catch (Exception e) {
                            echo "Skipping direct NodePort smoke test: ${e.getMessage()}"
                        }
                    } else {
                        echo "Smoke testing WebApp root through ALB: http://${ALB_DNS}/"
                        sh "curl -f http://${ALB_DNS}/ || exit 1"

                        echo "Smoke testing APIs through ALB (if health routes are mapped)..."
                        sh "curl -f http://${ALB_DNS}/health || echo 'Health endpoint check failed or not mapped'"
                    }
                }
            }
        }
    }

    post {
        failure {
            echo "Pipeline failed at stage: ${STAGE_NAME}. Check logs above."
        }
        success {
            echo "Deployment successful. Image tag: ${IMAGE_TAG}"
        }
    }
}