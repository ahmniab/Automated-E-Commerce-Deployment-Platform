node  {

    stage('Deploy Changed Services') {
        def jops = sh(script: '/usr/bin/python ./src/scripts/get-deployment-jops', returnStdout: true).trim().split('\n')
        echo "current path: ${env.WORKSPACE}"
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