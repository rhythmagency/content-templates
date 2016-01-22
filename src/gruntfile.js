module.exports = function(grunt) {

    // Initialize Grunt tasks.
    grunt.initConfig({
        "pkg": grunt.file.readJSON('package.json'),
		umbracoPackage: {
            main: {
                src: "./files",
                dest: "../dist",
                options: {
                    name: "ContentTemplates",
                    version: "0.1.0",
                    url: "https://github.com/rhythmagency/content-templates",
                    license: "MIT License",
                    licenseUrl: "http://opensource.org/licenses/MIT",
                    author: "Rhythm Agency",
                    authorUrl: "http://rhythmagency.com/",
                    readme: grunt.file.read("templates/inputs/readme.txt"),
                    manifest: "templates/package.template.xml"
                }
            }
        }
    });

    // Load NPM tasks.
    grunt.loadNpmTasks("grunt-umbraco-package");

    // Register Grunt tasks.
    grunt.registerTask("default",
        [ "umbracoPackage:main" ]);

};