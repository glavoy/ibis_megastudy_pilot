# ibis_megastudy_pilot
# Changesets
# version 2026-03-16
# Dropped Unprotected Sex as one of the eligibility requirements for Kenya only

# verison 2026-04-15
# Fixed the QC report query
# Added the error in skip pattern in followup crf that was causing the dice_clinic question was being skipped
# Added the Retesting/QC data entry form and report
# Improved Search functionality to allow searching by IBIS ID and National ID

# version 2026.04.23
# Fix primary endpoint visit check error. Dropped the time from the date coz it could be causing the error in checking the date range

# version 2026.05.06
# Add dob to the baseline_lookup table and add it the duplicate names QC
# Add Participant Study arm as part of the information displayed on the dashboard

# version 2026.05.07
# Added New columns to the baseline table county_rd, subcounty_rd, and started_prep_pep
# Updated entry point question added Outreach and VCT to the list of options
# Dropped Dice from the surveys both Baseline and Followup - added so that the question is not asked

# version 2026.06.15
# Updating Retesting QC crf. Added column to document data source