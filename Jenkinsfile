node  {

    stage('Deploy Changed Services') {
        echo "current path: ${env.WORKSPACE}"
        def jops = sh(script: '/usr/bin/python ./src/scripts/get-deployment-jops', returnStdout: true).trim().split('\n')
        for (jop in jops) {
            def jopDetails = jop.split(':')
            def jopPath = jopDetails[0]
            env.VERSION = jopDetails[1]
            try {
                load jopPath
            } catch (Exception e) {
                echo "Error loading ${jopPath}: ${e.getMessage()}"
            }
        }
        
    }
    
}