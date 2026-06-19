node  {
    stage('Checkout Code') {
        checkout scm
    }
    stage('Deploy Changed Services') {
       
        def scriptOutput = sh(script: '/usr/bin/python ./src/scripts/get-deployment-jops', returnStdout: true).trim()
        
        if (scriptOutput.isEmpty()) {
            echo "No services to deploy."
            return 
        }

        def jops = scriptOutput.split('\n')

        stage('Login to docker') {
            withCredentials([usernamePassword(credentialsId: 'dockerhub-credentials', usernameVariable: 'DOCKERHUB_USERNAME', passwordVariable: 'DOCKERHUB_PASSWORD')]) {
                sh 'echo $DOCKERHUB_PASSWORD | docker login -u $DOCKERHUB_USERNAME --password-stdin'
            }
        }

        for (jop in jops) {
            def jopDetails = jop.split(':')
            if (jopDetails.length >= 2) {
                def jopPath = jopDetails[0].trim() // Added trim() to clean up stray spaces
                env.VERSION = jopDetails[1].trim()
                
                try {
                    load jopPath
                } catch (Exception e) {
                    echo "❌ Error loading ${jopPath}: ${e.getMessage()}"
                }
            } else {
                echo "Skipping malformed line: '${jop}'"
            }
        }
    }
}