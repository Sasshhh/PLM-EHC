import re

files = [
    r'C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\Helpers\MatchingHelper.cs',
    r'C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\Controllers\DocumentController.cs'
]

for file_path in files:
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Replace .Include(o => o.File)
    content = content.replace('.Include(o => o.File)', '')
    
    # Replace the if block
    old_block = '''if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));'''
    new_block = '''if (customerDocument.FileId != null)
                {
                    customerDocument.File = new C8.eServices.Mvc.Models.File { CreatedDateTime = customerDocument.CreatedDateTime };
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));
                }'''
                
    content = content.replace(old_block, new_block)

    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)

print('Files updated successfully.')
