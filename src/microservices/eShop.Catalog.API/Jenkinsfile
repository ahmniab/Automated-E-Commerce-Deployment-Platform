pipeline {
    agent any

    environment {
        IMAGE_NAME = "eshop/catalog-api"
        IMAGE_TAG = "latest"

        DOCKERFILE_TEST = "Dockerfile.test"
        DOCKERFILE_APP = "src/microservices/eShop.Catalog.API/Dockerfile"
        CONTEXT_PATH = "."
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Test (via Docker)') {
            steps {
                sh "docker build -f ${DOCKERFILE_TEST} ${CONTEXT_PATH}"
            }
        }

        stage('Build Image') {
            steps {
                sh """
                    docker build \
                    -t ${IMAGE_NAME}:${IMAGE_TAG} \
                    -f ${DOCKERFILE_APP} \
                    ${CONTEXT_PATH}
                """
            }
        }

        stage('Push to DockerHub') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'dockerhub-creds', usernameVariable: 'USER', passwordVariable: 'PASS')]) {
                    sh """
                        echo $PASS | docker login -u $USER --password-stdin
                        docker push ${IMAGE_NAME}:${IMAGE_TAG}
                    """
                }
            }
        }
    }

    post {
        success {
            echo "Pipeline SUCCESS 🚀"
        }
        failure {
            echo "Pipeline FAILED ❌"
        }
    }
}
