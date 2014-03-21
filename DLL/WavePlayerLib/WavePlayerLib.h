#pragma once
#include "stdafx.h"
#include <stdio.h>
#include <math.h>
#include <windows.h>
#include <winbase.h>
#include <mmsystem.h>
#include <iostream>
#include <fstream>
#include <string>
#include <stdexcept> 
using namespace std;
#pragma comment(lib, "winmm.lib")
#define BUFFERS			3				// no. of buffers, adjust if necessary 
int BUFFER_SIZE;
HMMIO           hmmioIn;
MMCKINFO        pckInRIFF;				// WAVE chunk
MMCKINFO        ckIn;					// chunk info. for general use.
PCMWAVEFORMAT   pcmWaveFormat;			// Temp PCM structure to load in. 
WAVEFORMATEX*	extendedFormat;			// Extended WAVE format 
WAVEHDR			databuffer[BUFFERS];	// buffers
HWAVEOUT	hAudioOut;		// handle to the playback device
int				finished = 0;			// Finsihed playback the file?
int				bytesPerSec;
int				seconds = 0;
int produced = 0;
int consumed = 1;
int pause = 0;
int volume = 0xFFFF;	//max
int duration = 0;
int secDiv = 8;
namespace WavePlayer
{
	extern "C" { __declspec(dllexport) void setVolume(int simpleVol); }
	extern "C" { __declspec(dllexport) int currentSec(); }
	extern "C" { __declspec(dllexport) int totalDuration(); }
	extern "C" { __declspec(dllexport) void PauseWave(); }
	extern "C" { __declspec(dllexport) void ResumeWave(); }
	extern "C" { __declspec(dllexport) void skipTo(int secPosition); }
	extern "C" { __declspec(dllexport) void StopWave(); }
	extern "C" { __declspec(dllexport) int PlayWave(LPTSTR filename); }

}