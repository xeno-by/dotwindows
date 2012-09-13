import sys, re, json
from restkit import Resource

if len(sys.argv) != 2:
  print "usage: " + sys.argv[0] + " <label>"
  print "where label is something like \"scalamacros:ticket/6323\""
  print "(note that the label shouldn't include the name of the repository!"
  sys.exit(1)
label = sys.argv[1]

pulls = json.loads(Resource("https://api.github.com/repos/scala/scala/pulls").get().body_string())
relevant_pulls = [pull for pull in pulls if pull["head"]["label"] == label]
if len(relevant_pulls) != 1: sys.exit(100)
pull = relevant_pulls[0]

comments = json.loads(Resource("https://api.github.com/repos/scala/scala/issues/" + str(pull["number"]) + "/comments").get().body_string())
relevant_comments = [comment for comment in comments if comment["user"]["login"] == "scala-jenkins"]
for comment in reversed(relevant_comments):
  pattern = "jenkins job pr-scala-testsuite-linux-opt: (.*?) - (.*)"
  m = re.match(pattern, comment["body"])
  if m:
    status = m.group(1)
    url = m.group(2) + "/consoleText"
    if status == "Failed":
      lines = Resource(url).get().body_string().split("\n")
      marker = "BUILD FAILED"
      if marker in lines:
        prefix = "/localhome/jenkins/b/workspace/pr-scala-testsuite-linux-opt/"
        suffix = " [FAILED]"
        relevant_lines = [line for line in lines[lines.index(marker):] if line.startswith(prefix)][2:]
        if relevant_lines:
          for line in relevant_lines:
            print line[len(prefix):-len(suffix)]
          sys.exit(103)
    elif status == "Success":
      sys.exit(102)
    else:
      raise Exception("unknown build status in " + comment["body"])

sys.exit(101)
