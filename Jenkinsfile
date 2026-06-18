node  {
    stage('Checkout Code') {
        checkout scm
    }
    stage('Deploy Changed Services') {
        echo "current path: ${env.WORKSPACE}"
        
        sh 'echo $(ls)' 
        
        def jops = sh(script: '/usr/bin/python ./src/scripts/get-deployment-jops', returnStdout: true).trim().split('\n')
        
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