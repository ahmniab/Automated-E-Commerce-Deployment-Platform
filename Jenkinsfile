node  {

    stage('Deploy Changed Services') {
        def jops = sh(script: './src/scripts/get-deployment-jop', returnStdout: true).trim().split('\n')
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