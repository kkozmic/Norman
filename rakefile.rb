require 'rake'
require 'albacore'

DOT_NET_PATH = "#{ENV["SystemRoot"]}\\Microsoft.NET\\Framework\\v4.0.30319"

ENV["config"] = "Debug" if ENV["config"].nil?
CONFIG = ENV["config"]

task :default => [:compile, :test]

desc "Compile solution"
msbuild :compile do |msb|
    msb.properties :configuration => CONFIG
    msb.command = File.join(DOT_NET_PATH, "msbuild.exe")
    msb.targets :Clean, :Build
    msb.solution = "Norman.sln"
end

xunit :test => [:compile] do |xunit|
    xunit.command = "lib/xunit.console.clr4.exe"
    xunit.assembly = "tst/Norman.Tests/bin/#{CONFIG}/Norman.Tests.dll"
end
