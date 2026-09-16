from pathlib import Path

class FileRegionReplacer:
    def __init__(self, file_path: str):
        self.file_path = file_path
        self.content = Path(file_path).read_text(encoding='utf-8')

    def replace_region(self, region_name, region_content):
        start_tag = f"//{{{{{region_name}"
        end_tag = f"//}}}}{region_name}"
        # print(start_tag, end_tag)
        start_index = self.content.find(start_tag)
        end_index = self.content.find(end_tag)
        end_index = self.content[:end_index].rfind('\n')

        if start_index == -1 or end_index == -1:
            raise ValueError(f"Region tags for '{region_name}' not found in the file.")

        self.content = "".join([self.content[0:start_index], start_tag, '\n', region_content, '\n', self.content[end_index:]])

    def save(self, output_path=None):
        target_path = output_path if output_path else self.file_path
        Path(target_path).write_text(self.content, encoding='utf-8')

