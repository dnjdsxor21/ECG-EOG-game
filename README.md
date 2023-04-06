# ECG-EOG-game
### Simple t-rex game controlled by ECG and EOG

![game_gif](https://user-images.githubusercontent.com/75467530/230481366-1f627547-fa07-401d-8bc6-11f224a45bc2.gif)

# ECG
3-lead electordes were positioned.

![image](https://user-images.githubusercontent.com/75467530/230487219-329eb660-5787-4676-b231-54ddaafa5577.png)


We follow the [Pan Tompskins Algorithm](https://kr.mathworks.com/matlabcentral/fileexchange/45840-complete-pan-tompkins-implementation-ecg-qrs-detector) to implement ECG QRS Complex detector.

![image](https://user-images.githubusercontent.com/75467530/230486414-8376a7cc-0ec9-4d30-a91b-a0c71a95dc11.png)


So bpm come from **R-R intervals**.

$BPM = \frac{60 * f_s}{mean-of-R-R-intervals}$


# EOG
3-lead electoreds were positioned.

The two electrodes were attached to the up-and-down side of left eye.

And the**ground** was on the left side of left eye.

![image](https://user-images.githubusercontent.com/75467530/230487631-4650bf8f-226b-46e6-b07d-32f2b1afad93.png)

The t-rex character move up(down) when the eyeballs go up(down).

# Analog Filter

Use the active band-pass filter (0.159Hz ~ 15.9Hz) to **pass only LOW frequency.**

# Digital Filter / MSP 430

sample-rate = 250Hz

ECG : 3rd order Butterworth bandpass filter (5~30Hz) to remove the power noise
EOG : 3rd order Butterworth bandpass filter (5~10Hz) to remove the power noise

# GUI
C# .NET used.
[Mitov PlotLab Basic](https://mitov.com/products/plotlab#overview) used.
- The speed of the obstacle is determined by **bpm**.
- The character moves with **the movement of the eyeballs**.

# ~~Matlab~~

At first, We implement the preprocessing algorithm(pan tompkins) in the [Matlab](https://kr.mathworks.com/?s_tid=gn_logo).

But, CPU memory was insufficient.

# Reference
Biopac(https://www.biopac.com/application-note/ecg-ekg-electrocardiography-12-6-3-lead/)

ECG(https://journals.pan.pl/dlibra/publication/118163/edition/102773/content)

EOG(https://electrooculography.wordpress.com/)

Pan Tomkins(https://kr.mathworks.com/matlabcentral/fileexchange/45840-complete-pan-tompkins-implementation-ecg-qrs-detector)

Mitov Plotlab(https://mitov.com/products/plotlab#overview)

T-rex game(https://github.com/mooict/T-Rex-Endless-Runner-Game-in-Windows-Form)


