// will download and run sonarr (server) in a non-windows enviroment
// you can use this if you don't care about the server code and just want to work
// with the web code.

var http = require('http');
var gulp = require('gulp');
var fs = require('fs');
var targz = require('tar.gz');
var del = require('del');
var print = require('gulp-print');
var spawn = require('child_process').spawn;

function download(url, dest, cb) {
  console.log('Downloading ' + url + ' to ' + dest);
  var file = fs.createWriteStream(dest);
  var request = http.get(url, function (response) {
    response.pipe(file);
    file.on('finish', function () {
      console.log('Download completed');
      file.close(cb);
    });
  });
}

function getLatest(cb) {
  var branch = 'develop';
  process.argv.forEach(function (val) {
    var branchMatch = /branch=([\S]*)/.exec(val);
    if (branchMatch && branchMatch.length > 1) {
      branch = branchMatch[1];
    }
  });

  var url = 'http://services.sonarr.tv/v1/update/' + branch + '?os=osx';

  console.log('Checking for latest version:', url);

  http.get(url, function (res) {
    var data = '';

    res.on('data', function (chunk) {
      data += chunk;
    });

    res.on('end', function () {
      var updatePackage = JSON.parse(data).updatePackage;
      console.log('Latest version available: ' + updatePackage.version + ' Release Date: ' + updatePackage.releaseDate);
      cb(updatePackage);
    });
  }).on('error', function (e) {
    console.log('problem with request: ' + e.message);
  });
}

function extract(source, dest, cb) {
  console.log('extracting download page to ' + dest);
  new targz().extract(source, dest, function (err) {
    if (err) {
      console.log(err);
    }
    console.log('Update package extracted.');
    cb();
  });
}

gulp.task('getSonarr', function () {

  //gulp.src('/Users/kayone/git/Sonarr/_start/2.0.0.3288/NzbDrone/*.*')
  //  .pipe(print())
  //  .pipe(gulp.dest('./_output

  //return;
  try {
    fs.mkdirSync('./_start/');
  } catch (e) {
    if (e.code != 'EEXIST') {
      throw e;
    }
  }

  getLatest(function (package) {
    var packagePath = "./_start/" + package.filename;
    var dirName = "./_start/" + package.version;
    download(package.url, packagePath, function () {
      extract(packagePath, dirName, function () {
        // clean old binaries
        console.log('Cleaning old binaries');
        del.sync(['./_output/*', '!./_output/UI/']);
        console.log('copying binaries to target');
        gulp.src(dirName + '/NzbDrone/*.*')
          .pipe(gulp.dest('./_output/'));
      });
    });
  });
});

gulp.task('startSonarr', function () {

  var ls = spawn('mono', ['--debug', './_output/NzbDrone.exe']);

  ls.stdout.on('data', function (data) {
    process.stdout.write('' + data);
  });

  ls.stderr.on('data', function (data) {
    process.stdout.write('' + data);
  });

  ls.on('close', function (code) {
    console.log('child process exited with code ' + code);
  });
});
