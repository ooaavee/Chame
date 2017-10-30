/// <binding AfterBuild='less' />
var gulp = require('gulp'),
    less = require('gulp-less'),
    cssmin = require('gulp-cssmin'),
    rename = require('gulp-rename');

gulp.task('site', function () {
    return gulp.src('ClientApp/less/site.less')
        .pipe(less().on('error', function (err) {
            console.log(err);
        }))
        .pipe(cssmin().on('error', function (err) {
            console.log(err);
        }))
        .pipe(gulp.dest('wwwroot/css/'));
});

gulp.task('bootstrap', function () {
    return gulp.src('ClientApp/less/bootstrap.less')
        .pipe(less().on('error', function (err) {
            console.log(err);
        }))
        .pipe(cssmin().on('error', function (err) {
            console.log(err);
        }))
        .pipe(gulp.dest('wwwroot/css/'));
});

gulp.task('fontawesome', function () {
    return gulp.src('ClientApp/less/fontawesome.less')
        .pipe(less().on('error', function (err) {
            console.log(err);
        }))
        .pipe(cssmin().on('error', function (err) {
            console.log(err);
        }))
        .pipe(gulp.dest('wwwroot/css/'));
});

gulp.task('default', ['site', 'bootstrap', 'fontawesome']);