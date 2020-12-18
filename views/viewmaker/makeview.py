
import sys
import fileinput
import re

def modify_post_in_place(file_name, pattern, value=""):
    f = open(file_name, "r")
    contents = f.readlines()
    f.close()

    idx = -1

    for line in contents:
      if pattern in line:
        idx = contents.index(line) + 1
        value = '\n' + value

    if idx == -1:
      return

    contents.insert(idx, value)

    f = open(file_name, "w")
    contents = "".join(contents)
    f.write(contents)
    f.close()

def print_file(file, view_name):
    f = open(file, "r")
    contents = f.readlines()
    f.close()
    print(contents)
    print('----------------------------------------')

    out_contents = []
    for line in contents:
      new_line = re.sub(r'\bI_View\b', view_name, line)
      new_line = re.sub(r'\bI_ViewModel\b', view_name + 'Model', new_line)
      out_contents.append(new_line)

    print(out_contents)

def copy_file_txt(file1, file2, view_name):
    f = open(file1, "r")
    contents = f.readlines()
    f.close()

    out_contents = []
    for line in contents:
      new_line = re.sub(r'\bI_View\b', view_name, line)
      new_line = re.sub(r'\bI_ViewModel\b', view_name + 'Model', new_line)
      out_contents.append(new_line)


    f = open(file2, "w")
    f.writelines(out_contents)
    f.close()


def Main():
    view_name = sys.argv[1]
    if(view_name.find('View') == -1):
      return

    comp_inc = """<Compile Include="./views/""" + view_name + """.xaml.cs">
  <DependentUpon>""" + view_name + """.xaml</DependentUpon>
  </Compile>"""

    page_inc = """<Page Include="./views/""" + view_name + """.xaml">
    <SubType>Designer</SubType>
    <Generator>MSBuild:Compile</Generator>
    </Page>\n"""

    modify_post_in_place("../../Build.csproj", '<!-- ###PAGE INCLUDE### -->', page_inc)
    modify_post_in_place("../../Build.csproj", '<!-- ###COMPILE INCLUDE### -->', comp_inc)

    #print_file("./cp_files/I_ViewCmds.cs", view_name)

    copy_file_txt("./cp_files/I_ViewCmds.cs", '../../commands/' + view_name + 'Cmds.cs', view_name)
    copy_file_txt("./cp_files/I_ViewModel.cs", '../../viewmodels/' + view_name + 'Model.cs', view_name)
    copy_file_txt("./cp_files/I_View.xaml", '../../views/' + view_name + '.xaml', view_name)
    copy_file_txt("./cp_files/I_View.xaml.cs", '../../views/' + view_name + '.xaml.cs', view_name)


if __name__ == '__main__':
    Main()
