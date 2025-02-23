#pragma once

#include "pch.h"

extern "C"
{
#include <libavformat\avformat.h>
}
#include <ppltasks.h>
#include "AttachedFile.h"


using namespace winrt::Windows::Foundation;
using namespace winrt::Windows::Storage;
using namespace winrt::Windows::Storage::Streams;

class AttachedFileHelper
{
public:
    virtual ~AttachedFileHelper()
    {
        if (extractedFiles.Size() > 0)
        {
            CleanupTempFiles(config.General().AttachmentCacheFolderName(), InstanceId());
        }
    }

    AttachedFileHelper(winrt::FFmpegInteropX::MediaSourceConfig const& config)
    {
        this->config = config;
    }

    std::vector<std::shared_ptr<AttachedFile>> AttachedFiles() { return attachedFiles; }
    winrt::hstring InstanceId()
    {
        if (this->instanceId.empty())
        {
            GUID gdn;
            auto hr = CoCreateGuid(&gdn);
            if (FAILED(hr))
            {
                winrt::throw_hresult(hr);
            }
            instanceId = winrt::to_hstring(winrt::guid(gdn));
        }
        return instanceId;
    }

    void AddAttachedFile(std::shared_ptr<AttachedFile> const& file)
    {
        attachedFiles.push_back(file);
    }

    winrt::Windows::Foundation::IAsyncOperation<winrt::Windows::Storage::StorageFile> ExtractFileAsync(std::shared_ptr<AttachedFile> attachment)
    {
        StorageFile file = { nullptr };
        if (extractedFiles.HasKey(attachment->Name()))
        {
            file = extractedFiles.Lookup(attachment->Name());
        }
        else
        {
            auto folder = co_await ApplicationData::Current().TemporaryFolder().CreateFolderAsync(
                config.General().AttachmentCacheFolderName(), CreationCollisionOption::OpenIfExists);
            auto instanceFolder = co_await folder.CreateFolderAsync(InstanceId(), CreationCollisionOption::OpenIfExists);
            file = (co_await instanceFolder.CreateFileAsync(attachment->Name(), CreationCollisionOption::ReplaceExisting));
            co_await FileIO::WriteBufferAsync(file, attachment->GetBuffer());

            extractedFiles.Insert(attachment->Name(), file);
        }
        co_return file;
    };

    static winrt::Windows::Foundation::IAsyncAction CleanupTempFiles(winrt::hstring folderName, winrt::hstring instanceId)
    {
        try
        {
            auto folder = co_await ApplicationData::Current().TemporaryFolder().GetFolderAsync(folderName);
            auto instancefolder = co_await folder.GetFolderAsync(instanceId);
            auto files = co_await instancefolder.GetFilesAsync();
            for (auto file : files)
            {
                co_await file.DeleteAsync();
            }
            co_await instancefolder.DeleteAsync();
        }
        catch (...)
        {
            OutputDebugString(L"Failed to cleanup temp files.");
        }
    }

    winrt::Windows::Foundation::IAsyncOperation<winrt::Windows::Storage::StorageFolder> GetInstanceFolder()
    {
        auto folder = co_await ApplicationData::Current().TemporaryFolder().CreateFolderAsync(
            config.General().AttachmentCacheFolderName(), CreationCollisionOption::OpenIfExists);
        auto instanceFolder = co_await folder.CreateFolderAsync(InstanceId(), CreationCollisionOption::OpenIfExists);
        co_return instanceFolder;
    }

private:
    winrt::Windows::Foundation::Collections::IMap<winrt::hstring, winrt::Windows::Storage::StorageFile> extractedFiles{ winrt::single_threaded_map<winrt::hstring, winrt::Windows::Storage::StorageFile>() };
    std::vector<std::shared_ptr<AttachedFile>> attachedFiles;
    winrt::FFmpegInteropX::MediaSourceConfig config = { nullptr };
    winrt::hstring instanceId{};
};

