// WavePlayerLib.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "WavePlayerLib.h" 

namespace WavePlayer {
	void setVolume(int simpleVol){ //0 - 100
		volume = (int)(65535 * (simpleVol / 100.0));
		//printf("[VOLUME:%d]\n", volume);
	}
	int totalDuration(){
		return duration;
	}
	int currentSec(){
		return (int)(consumed/secDiv);
	}
	void PauseWave(){
		waveOutPause(hAudioOut);
	}
	void ResumeWave(){
		waveOutRestart(hAudioOut);
	}
	void skipTo(int secPosition)
	{
		PauseWave();
		seconds = secPosition*secDiv;
		mmioSeek(hmmioIn, BUFFER_SIZE * seconds, SEEK_SET);
		consumed = seconds - 2;

		ResumeWave();
	}
	void CALLBACK playSamples(HWAVEOUT hwo, UINT m, DWORD i, DWORD p1, DWORD p2)
	{
		static int last = ckIn.cksize / BUFFER_SIZE;
		if (finished == 1){
			return;
		}

		// this function is called while finish playing one buffer
		//  so fill in the next here using waveOutOpen
		waveOutSetVolume(hAudioOut, volume);
		fprintf(stderr, "Entered %d/%d, Produced:%d\n", consumed, last, produced);
		//while (pause == 1);
		if (m == WOM_DONE) {
			if (seconds < last) {
				int which = produced%BUFFERS;

				//printf("[%d][%d]\n", seconds, mmioSeek(hmmioIn, (bytesPerSec * seconds ), SEEK_SET));

				if (mmioRead(hmmioIn, (databuffer[which].lpData), BUFFER_SIZE) != BUFFER_SIZE) {
					fprintf(stderr, "Error reading wave data\n");
					mmioClose(hmmioIn, 0);
					return;
				}

				waveOutWrite(hwo, &databuffer[which], sizeof(WAVEHDR));

				seconds++;
				produced++;
			}

			if (++consumed == last) {
				finished = 1;
				return;
			}
		}
	}

	int readWaveFile(LPTSTR filename)
	{
		WORD  cbExtraAlloc;   // Extra bytes for waveformatex 

		// use mmioOpen to open a ".wav" file
		finished = 0;
		seconds = 0;
		produced = 0;
		consumed = 1;
		pause = 0;
		printf("%s\n", filename);
		if ((hmmioIn = mmioOpen((LPTSTR)filename, NULL, MMIO_READ)) == NULL) {
			fprintf(stderr, "Error: mmioOpen error on %s\n", filename);
			return -1;
		}

		// find the chunk that is of WAVE type in RIFF chunks

		pckInRIFF.fccType = mmioFOURCC('W', 'A', 'V', 'E');
		if (mmioDescend(hmmioIn, &pckInRIFF, NULL, MMIO_FINDRIFF)) {
			fprintf(stderr, "Error: could not find WAVE chunk or ");
			fprintf(stderr, "the file is not in RIFF\n");
			mmioClose(hmmioIn, 0);
			return -1;
		}

		printf("File is a RIFF file containing WAVE chunk\n");

		// Now, descended into a WAVE chunk
		//      next, find the chunk with ckid = fmt

		ckIn.ckid = mmioFOURCC('f', 'm', 't', ' ');
		if (mmioDescend(hmmioIn, &ckIn, &pckInRIFF, MMIO_FINDCHUNK)) {
			fprintf(stderr, "Error: Could not locate 'fmt' chunk, problem in file format?\n");
			mmioClose(hmmioIn, 0);
			return -1;
		}

		// expect that the size of fmt data is not less than PCMWAVEFORMAT

		if (ckIn.cksize < (long) sizeof(PCMWAVEFORMAT)) {
			fprintf(stderr, "Error, 'fmt' chunk too small, problem in file?\n");
			mmioClose(hmmioIn, 0);
			return -1;
		}

		// Assume it is a PCM wave file, 
		// read the PCMWAVEFORMAT using mmioRead

		const long waveformatSize = (long)sizeof(PCMWAVEFORMAT);
		if (mmioRead(hmmioIn, (HPSTR)&pcmWaveFormat, waveformatSize) != waveformatSize) {
			fprintf(stderr, "Error in reading the waveformat\n");
			mmioClose(hmmioIn, 0);
			return -1;
		}

		if (pcmWaveFormat.wf.wFormatTag == WAVE_FORMAT_PCM) {
			// if it is indeed a PCM wave file
			cbExtraAlloc = 0;
			printf("The file is a PCM wave file\n");
		}
		else {
			// if it isn't, read in length of extra bytes.

			printf("The file is NOT a PCM wave file\n");

			const long wSize = (long)sizeof(cbExtraAlloc);
			if (mmioRead(hmmioIn, (LPSTR)&cbExtraAlloc, wSize) != wSize) {
				fprintf(stderr, "Error in reading extra bytes\n");
				mmioClose(hmmioIn, 0);
				return -1;
			}

			// the file is not a PCM wave file, but a compressed(?) wave file
			// the format is contained in WAVEFORMATEX structure

			extendedFormat = (WAVEFORMATEX *)GlobalAlloc(GMEM_FIXED, sizeof(WAVEFORMATEX)+cbExtraAlloc);
			if (extendedFormat == NULL) {
				fprintf(stderr, "Could not allocate memory from the heap!\n");
				return -1;
			}

			// Copy the bytes from the pcm structure to the waveformatex structure

			memcpy(extendedFormat, &pcmWaveFormat, sizeof(pcmWaveFormat));
			extendedFormat->cbSize = cbExtraAlloc;

			// Readin the extra data

			if (mmioRead(hmmioIn, (LPSTR)(((BYTE *)&(extendedFormat->cbSize) + (int)wSize)),
				(long)cbExtraAlloc) != (long)cbExtraAlloc) {
				fprintf(stderr, "Error in reading extra %d bytes\n", cbExtraAlloc);
				mmioClose(hmmioIn, 0);
				return -1;
			}

			// no problem, but this kind of wave file could not be handled by
			// this program

			fprintf(stderr, "Could not handle compressed wave file, program aborts!\n");
			return -1;
		}

		// finished reading the format information, escape from 'fmt' chunk

		if (mmioAscend(hmmioIn, (LPMMCKINFO)&ckIn, 0)) {
			fprintf(stderr, "Error in mmioAscend\n");
			mmioClose(hmmioIn, 0);
			return -1;
		}

		// Now, proceed to the data chunk

		ckIn.ckid = mmioFOURCC('d', 'a', 't', 'a');
		if (mmioDescend(hmmioIn, &ckIn, &pckInRIFF, MMIO_FINDCHUNK)) {
			fprintf(stderr, "Error: Could not locate 'data' chunk, problem in file format?\n");
			mmioClose(hmmioIn, 0);
			return -1;
		}

		// Displaying Wave file information

		char *channels[3] = { "???", "Mono", "Stereo" };
		WAVEFORMAT *wf = &(pcmWaveFormat.wf);
		bytesPerSec = (wf->nSamplesPerSec*wf->nBlockAlign);
		BUFFER_SIZE = bytesPerSec/secDiv;
		double seconds = (double)ckIn.cksize / (wf->nSamplesPerSec*wf->nBlockAlign);
		duration = (int)seconds;
		int min = (int)(seconds / 60);
		int sec = (int)(seconds - 60 * min);
		double hun = (seconds - floor(seconds)) * 100;

		printf("Ready to read the wave data\n");
		printf("------------------------------------\n");
		printf("-------Bytes per seconds: %d-------\n", bytesPerSec);
		printf("File format: %d Hz, %s, %d-bit\n", wf->nSamplesPerSec, channels[wf->nChannels], pcmWaveFormat.wBitsPerSample);
		printf("Total playing time is: %d:%d:%0.2f seconds\n", min, sec, hun);
		printf("------------------------------------\n");
		return 0;
	}
	void StopWave(){

		// close mmio handle, and close the audio device
		finished = 1;
		waveOutPause(hAudioOut);
	}
	int PlayWave(LPTSTR filename)
	{
		int			i, err;
		char		errorMsg[200];




		readWaveFile(filename);

		// Open the audio playback device

		err = waveOutOpen(&hAudioOut, WAVE_MAPPER, (WAVEFORMATEX *)&pcmWaveFormat, (DWORD)playSamples, 0L, CALLBACK_FUNCTION);
		waveOutSetVolume(hAudioOut, volume);
		if (err) {
			waveOutGetErrorText(err, errorMsg, sizeof(errorMsg));
			fprintf(stderr, "Error in WaveOutOpen: %s\n", errorMsg);
			return -1;
		}

		// prepare the audio buffers

		for (i = 0; i < BUFFERS; i++) {
			databuffer[i].lpData = new char[BUFFER_SIZE];
			databuffer[i].dwBufferLength = BUFFER_SIZE;
			databuffer[i].dwFlags = 0;
			memset(databuffer[i].lpData, 0, BUFFER_SIZE);
			err = waveOutPrepareHeader(hAudioOut, &databuffer[i], sizeof(WAVEHDR));

			if (err) {
				waveOutUnprepareHeader(hAudioOut, &databuffer[i], sizeof(WAVEHDR));
				waveOutClose(hAudioOut);
				waveOutGetErrorText(err, errorMsg, sizeof(errorMsg));
				fprintf(stderr, "Error in WaveOutOpen: %s\n", errorMsg);
				return -1;
			}
		}

		// fill the audio buffers
		//mmioSeek(hmmioIn, bytesPerSec*100 + 44, SEEK_SET); 
		if (mmioRead(hmmioIn, (databuffer[0].lpData), BUFFER_SIZE) != BUFFER_SIZE) {
			fprintf(stderr, "Error reading wave data\n");
			mmioClose(hmmioIn, 0);
			return -1;
		}
		if (mmioRead(hmmioIn, (databuffer[1].lpData), BUFFER_SIZE) != BUFFER_SIZE) {
			fprintf(stderr, "Error reading wave data\n");
			mmioClose(hmmioIn, 0);
			return -1;
		}
			if (mmioRead(hmmioIn, (databuffer[2].lpData), BUFFER_SIZE) != BUFFER_SIZE) {
		fprintf(stderr, "Error reading wave data\n");
		mmioClose(hmmioIn, 0);
		return -1;
		}
		
		// Initially, play back 3 buffers of data
		//waveOutSetVolume(hAudioOut, volume);
		waveOutWrite(hAudioOut, &databuffer[0], sizeof(WAVEHDR));
		seconds++;
		produced++;
		//waveOutSetVolume(hAudioOut, volume);
		waveOutWrite(hAudioOut, &databuffer[1], sizeof(WAVEHDR));
		seconds++;
		produced++;
		waveOutWrite(hAudioOut, &databuffer[2], sizeof(WAVEHDR));
		seconds++;
		produced++;
		int once = 0;
		while (!finished) {
			// Do other things here!
			//  the program will continue to play by calling 
			//  the callback function


		}

		mmioClose(hmmioIn, 0);
		for (int i = 0; i < BUFFERS; i++) {
			waveOutUnprepareHeader(hAudioOut, &databuffer[i], sizeof(WAVEHDR));
		}
		waveOutClose(hAudioOut);

		return 0;
	}


}

