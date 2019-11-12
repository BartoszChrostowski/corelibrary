leancode.builder('corelibrary')
    .withCustomJnlp()
    .withDotnet(version: '3.0')
    .run {
    def scmVars

    stage('Checkout') {
        scmVars = safeCheckout scm
    }

    def isMasterBuild = BRANCH_NAME ==~ /v\d\.\d(\.\d)?/
    leancode.configureRepositories()

    stage('Version') {
        def baseVer = sh(
            script: 'cat CHANGELOG.md | awk \'$1$2 ~ /^##[0-9]\\.[0-9](\\.[0-9])?/ { print $2 }\' | head -n 1',
            returnStdout: true).trim()

        env.GIT_COMMIT = scmVars.GIT_COMMIT
        env.VERSION = isMasterBuild ? "${baseVer}.${nextBuildNumber()}" : '0.0.0'
        echo "Building version: ${env.VERSION}"

        if (isMasterBuild) {
            currentBuild.displayName = "v${env.APP_VERSION}"
        }
    }

    container('dotnet') {
        stage('Build') {
            sh 'dotnet restore'
            sh 'dotnet build -c Release --no-restore'
        }

        stage('Test') {
            try {
                dir('test') {
                    sh 'dotnet msbuild /t:RunTests /p:Configuration=Release /p:LogFileName=$PWD/test-results/tests.trx'
                }
            } finally {
                step([$class: 'MSTestPublisher', testResultsFile:'*/test-results/*.trx', failOnError: true, keepLongStdio: true])
            }
        }

        stage('Pack') {
            sh 'dotnet pack --no-restore -c Release -o $PWD/packed'
        }

        stage('Publish') {
            when (isMasterBuild) {
                withCredentials([string(credentialsId: 'LeanCodeMyGetApiKey', variable: 'MYGET_APIKEY')]) {
                    sh "dotnet nuget push -k '$MYGET_APIKEY' -s 'https://www.myget.org/F/leancode/api/v2/package' 'packed/*.nupkg'"
                }
            }
        }
    }
}
