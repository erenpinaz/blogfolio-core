module.exports = function (grunt) {
	grunt.initConfig({
		less: {
			dev: {
				options: {
					compress: false
				},
				files: [
					{ 'wwwroot/css/site.css': 'wwwroot/less/site/site.less' },
					{ 'wwwroot/css/admin.css': 'wwwroot/less/admin/admin.less' }
				]
			}
		},
		cssmin: {
			site: {
				files: {
					'wwwroot/css/site.min.css': 'wwwroot/css/site.css'
				}
			},
			admin: {
				files: {
					'wwwroot/css/admin.min.css': 'wwwroot/css/admin.css'
				}
			}
		},
		uglify: {
			options: {
				mangle: false
			},
			app: {
				files: [
					{
						flatten: false,
						expand: true,
						cwd: "wwwroot/js/",
						src: ["*.js", "!*.min.js"],
						dest: "wwwroot/js",
						ext: ".min.js",
						extDot: 'last'
					}
				]
			}
		}
	});

	grunt.loadNpmTasks('grunt-contrib-less');
	grunt.loadNpmTasks('grunt-contrib-cssmin');
	grunt.loadNpmTasks('grunt-contrib-uglify');

	grunt.registerTask('all', ['less', 'cssmin', 'uglify']);
};