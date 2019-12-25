
opt_parallel=
opt_version=
opt_mode=both
while getopts 'pv:m:r?h' c
do
  case $c in
    p)   opt_parallel=1 ;;
    v)   opt_version=$OPTARG ;;
    m)   opt_mode=$OPTARG ;;
    r)   opt_report=1 ;;
    ?|h) printf "Usage: %s [-p] [-v mono-ver] [-m sonarr|complete]\n" $0 
         printf " -p  run parallel\n"
         printf " -v  run specified mono version\n"
         printf " -m  run only mono-'complete' or 'sonarr' package variants\n"
         printf " -r  only report\n"
         exit 2
  esac
done
# NOTE:
# each container has a 1gb tmpfs mounted since it greatly speeds up the normally intensive db operations
# make sure that the docker host has enough memory to handle about ~300 MB per container, so 2-3 GB total
# excess goes to the swap and will slow down the entire system

MONO_VERSIONS=""

# Future versions
MONO_VERSIONS="$MONO_VERSIONS 6.10=preview-xenial"

# Semi-Supported versions
MONO_VERSIONS="$MONO_VERSIONS 6.8 6.6 6.4 6.0"

# Supported versions 
MONO_VERSIONS="$MONO_VERSIONS 5.20 5.18"

# Legacy unsupported versions (but appear to work)
MONO_VERSIONS="$MONO_VERSIONS 5.16 5.14 5.12"

# Legacy unsupported versions
MONO_VERSIONS="$MONO_VERSIONS 5.10 5.8 5.4 5.0"
#MONO_VERSIONS="$MONO_VERSIONS 4.8=stable-wheezy/snapshots/4.8"

if [ "$opt_version" != "" ]; then
    MONO_VERSIONS="$opt_version"
fi

mkdir -p ${PWD}/../../_tests_results

prepOne() {
    local MONO_VERSION_PAIR=$1

    MONO_VERSION_SPLIT=(${MONO_VERSION_PAIR//=/ })
    MONO_VERSION=${MONO_VERSION_SPLIT[0]}
    MONO_URL=${MONO_VERSION_SPLIT[1]:-"stable-xenial/snapshots/$MONO_VERSION"}

    echo "Building Test Docker for mono $MONO_VERSION"
    
    if [ "$opt_mode" != "sonarr" ]; then
        docker build -t sonarr-test-$MONO_VERSION --build-arg MONO_VERSION=$MONO_VERSION --build-arg MONO_URL=$MONO_URL --file mono/complete/Dockerfile mono
    fi

    if [ "$opt_mode" != "complete" ] && [ "$MONO_VERSION" != "5.0" ]; then    
        docker build -t sonarr-test-$MONO_VERSION-sonarr --build-arg MONO_VERSION=$MONO_VERSION --build-arg MONO_URL=$MONO_URL --file mono/sonarr/Dockerfile mono
    fi
}

runOne() {
    local MONO_VERSION_PAIR=$1

    MONO_VERSION_SPLIT=(${MONO_VERSION_PAIR//=/ })
    MONO_VERSION=${MONO_VERSION_SPLIT[0]}
        
    echo "Running Test Docker for mono $MONO_VERSION"

    if [ "$opt_mode" != "sonarr" ]; then
        dockerArgs="--rm"
        dockerArgs="$dockerArgs -v /${PWD}/../../_tests_linux:/data/_tests_linux:ro"
        dockerArgs="$dockerArgs -v /${PWD}/../../_output_linux:/data/_output_linux:ro"
        dockerArgs="$dockerArgs -v /${PWD}/../../_tests_results/mono-$MONO_VERSION:/data/_tests_results"
        dockerArgs="$dockerArgs --mount type=tmpfs,destination=//data/test,tmpfs-size=1g"
        docker run $dockerArgs sonarr-test-$MONO_VERSION
    fi

    if [ "$opt_mode" != "complete" ] && [ "$MONO_VERSION" != "5.0" ]; then   
        dockerArgs="--rm"
        dockerArgs="$dockerArgs -v /${PWD}/../../_tests_linux:/data/_tests_linux:ro"
        dockerArgs="$dockerArgs -v /${PWD}/../../_output_linux:/data/_output_linux:ro"
        dockerArgs="$dockerArgs -v /${PWD}/../../_tests_results/mono-$MONO_VERSION-sonarr:/data/_tests_results"
        dockerArgs="$dockerArgs --mount type=tmpfs,destination=//data/test,tmpfs-size=1g"
        docker run $dockerArgs sonarr-test-$MONO_VERSION-sonarr
    fi
    
    echo "Finished Test Docker for mono $MONO_VERSION"
}

if [ "$opt_report" != "1" ]; then

    if [ "$opt_parallel" == "1" ]; then
        for MONO_VERSION_PAIR in $MONO_VERSIONS; do
            prepOne "$MONO_VERSION_PAIR"
        done
    fi

    for MONO_VERSION_PAIR in $MONO_VERSIONS; do
        if [ "$opt_parallel" == "1" ]; then
            runOne "$MONO_VERSION_PAIR" &
        else
            prepOne "$MONO_VERSION_PAIR"
            runOne "$MONO_VERSION_PAIR"
        fi
    done

    if [ "$opt_parallel" == "1" ]; then
        echo "Waiting for all runs to finish"
        wait
        echo "Finished all runs"
    fi

fi

grep "<test-run" ../../_tests_results/**/*.xml | sed -r 's/.*?mono-([0-9.]+(-s)?).*?_([IU]).*?\.xml.*?failed="([0-9]*)".*/\1\t\3:\tfailed \4/g' | sort -V -t.